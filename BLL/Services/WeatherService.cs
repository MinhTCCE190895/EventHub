using BLL.Interfaces;
using System.Text.Json;
using System.Threading; // Added to use SemaphoreSlim to prevent Cache Stampede
using BusinessObjects.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class WeatherService(HttpClient httpClient, IMemoryCache cache, ILogger<WeatherService> logger) : IWeatherService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IMemoryCache _cache = cache;
    private readonly ILogger<WeatherService> _logger = logger;
    
    // Semaphore to lock threads, allowing only 1 request to call the external API at a time (FE-08 Cache Stampede)
    private static readonly SemaphoreSlim _weatherSemaphore = new(1, 1);


    public async Task<WeatherDTO?> GetWeatherAsync(string? location)
    {
        if (string.IsNullOrWhiteSpace(location)) return null;
        string normalized = NormalizeLocation(location);
        string cacheKey = $"weather_{normalized.ToLower().Replace(" ", "_")}";

        // Step 1: Quick cache check before locking to optimize read speed
        if (_cache.TryGetValue(cacheKey, out WeatherDTO? cachedWeather))
        {
            return cachedWeather;
        }

        // Step 2: Wait for Semaphore lock if multiple threads are calling wttr.in simultaneously
        await _weatherSemaphore.WaitAsync();
        try
        {
            // Re-check cache after locking (Double-checked locking) in case another thread already populated the cache
            if (_cache.TryGetValue(cacheKey, out cachedWeather))
            {
                return cachedWeather;
            }

            // Set a short timeout to prevent degrading user experience if the third-party API is congested
            _httpClient.Timeout = TimeSpan.FromSeconds(3);
            string url = $"https://wttr.in/{Uri.EscapeDataString(normalized)}?format=j1";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;
                
                var currentCondition = root.GetProperty("current_condition")[0];
                double temp = double.Parse(currentCondition.GetProperty("temp_C").GetString() ?? "25");
                string desc = currentCondition.GetProperty("weatherDesc")[0].GetProperty("value").GetString() ?? "Trong lành";
                double wind = double.Parse(currentCondition.GetProperty("windspeedKmph").GetString() ?? "10");
                int humidity = int.Parse(currentCondition.GetProperty("humidity").GetString() ?? "80");

                var weather = new WeatherDTO
                {
                    Location = normalized,
                    Temperature = temp,
                    Condition = TranslateCondition(desc),
                    WindSpeed = wind,
                    Humidity = humidity,
                    WeatherIconClass = MapConditionToIcon(desc),
                    FetchedAt = DateTime.UtcNow
                };

                // Cache for 30 minutes as required
                _cache.Set(cacheKey, weather, TimeSpan.FromMinutes(30));
                return weather;
            }
        }
        catch (Exception ex)
        {
            // Only log system error, do not throw to avoid crashing the page when the weather API encounters issues
            _logger.LogError(ex, "Lỗi khi gọi API thời tiết cho {Location}. Sẽ dùng fallback data.", location);
        }
        finally
        {
            // Release Semaphore
            _weatherSemaphore.Release();
        }

        // Create high-quality fallback data if the API errors out or network is down
        var fallbackWeather = GetFallbackWeather(normalized);
        _cache.Set(cacheKey, fallbackWeather, TimeSpan.FromMinutes(5)); // Shorter cache duration for fallback data
        return fallbackWeather;
    }

    public async Task<WeatherDTO?> GetWeatherForecastAsync(string? location, DateTime targetDate)
    {
        if (string.IsNullOrWhiteSpace(location)) return null;
        // Add 7 hours to align with Vietnam timezone when calculating date difference
        var today = DateTime.UtcNow.AddHours(7).Date;
        var targetDateLocal = targetDate.Date;
        var daysDifference = (targetDateLocal - today).Days;

        // Early exit if outside the 3-day forecast range to avoid unnecessary API requests since wttr.in only stores short-term forecast
        if (daysDifference < 0 || daysDifference > 2)
        {
            return null;
        }

        string normalized = NormalizeLocation(location);
        string cacheKey = $"weather_fc_day_{normalized.ToLower().Replace(" ", "_")}_{targetDateLocal:yyyyMMdd}";

        if (_cache.TryGetValue(cacheKey, out List<WeatherDTO>? cachedForecast) && cachedForecast != null && cachedForecast.Count > 0)
        {
            int index = GetClosestHourlyIndex(targetDate, cachedForecast.Count - 1);
            return cachedForecast[index];
        }

        await _weatherSemaphore.WaitAsync();
        try
        {
            if (_cache.TryGetValue(cacheKey, out cachedForecast) && cachedForecast != null && cachedForecast.Count > 0)
            {
                int index = GetClosestHourlyIndex(targetDate, cachedForecast.Count - 1);
                return cachedForecast[index];
            }

            _httpClient.Timeout = TimeSpan.FromSeconds(3);
            string url = $"https://wttr.in/{Uri.EscapeDataString(normalized)}?format=j1";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;
                
                if (root.TryGetProperty("weather", out var weatherList) && weatherList.ValueKind == JsonValueKind.Array)
                {
                    List<WeatherDTO>? targetResultList = null;
                    
                    // Iterate through all forecast days returned from the API to batch-cache them
                    foreach (var day in weatherList.EnumerateArray())
                    {
                        if (day.TryGetProperty("date", out var dateProp) && 
                            DateTime.TryParse(dateProp.GetString(), out var parsedDate))
                        {
                            var dateKey = parsedDate.Date;
                            var hourlyArray = day.GetProperty("hourly");
                            var dayForecastList = new List<WeatherDTO>();

                            // Iterate through all 8 time slots of the day (3-hour intervals) to get detailed forecast
                            foreach (var hourly in hourlyArray.EnumerateArray())
                            {
                                double temp = double.Parse(hourly.GetProperty("tempC").GetString() ?? day.GetProperty("avgtempC").GetString() ?? "28");
                                string desc = hourly.GetProperty("weatherDesc")[0].GetProperty("value").GetString() ?? "Trong lành";
                                double wind = double.Parse(hourly.GetProperty("windspeedKmph").GetString() ?? "10");
                                int humidity = int.Parse(hourly.GetProperty("humidity").GetString() ?? "75");

                                dayForecastList.Add(new WeatherDTO
                                {
                                    Location = normalized,
                                    Temperature = temp,
                                    Condition = TranslateCondition(desc),
                                    WindSpeed = wind,
                                    Humidity = humidity,
                                    WeatherIconClass = MapConditionToIcon(desc),
                                    FetchedAt = DateTime.UtcNow
                                });
                            }

                            // Cache the 8 hourly time slots for that day for 30 minutes
                            string loopCacheKey = $"weather_fc_day_{normalized.ToLower().Replace(" ", "_")}_{dateKey:yyyyMMdd}";
                            _cache.Set(loopCacheKey, dayForecastList, TimeSpan.FromMinutes(30));

                            if (dateKey == targetDateLocal)
                            {
                                targetResultList = dayForecastList;
                            }
                        }
                    }

                    if (targetResultList != null && targetResultList.Count > 0)
                    {
                        int index = GetClosestHourlyIndex(targetDate, targetResultList.Count - 1);
                        return targetResultList[index];
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gọi API dự báo thời tiết cho {Location} ngày {Date}.", location, targetDateLocal);
        }
        finally
        {
            _weatherSemaphore.Release();
        }

        // Create fallback data with 8 identical hourly slots to avoid index errors
        var fallbackWeather = GetFallbackWeather(normalized);
        var fallbackList = Enumerable.Repeat(fallbackWeather, 8).ToList();
        _cache.Set(cacheKey, fallbackList, TimeSpan.FromMinutes(5));
        
        int fallbackIndex = GetClosestHourlyIndex(targetDate, 7);
        return fallbackList[fallbackIndex];
    }

    // Match event time with the closest 3-hour forecast block from wttr.in
    private static int GetClosestHourlyIndex(DateTime targetDate, int maxIndex)
    {
        int hour = targetDate.Hour;
        
        int index = hour switch
        {
            >= 23 or < 2 => 0,   // 00:00 slot
            >= 2 and < 5 => 1,   // 03:00 slot
            >= 5 and < 8 => 2,   // 06:00 slot
            >= 8 and < 11 => 3,  // 09:00 slot
            >= 11 and < 14 => 4, // 12:00 slot
            >= 14 and < 17 => 5, // 15:00 slot
            >= 17 and < 20 => 6, // 18:00 slot
            _ => 7               // 21:00 slot
        };

        return Math.Clamp(index, 0, maxIndex);
    }


    private static string NormalizeLocation(string location)
    {
        // Null or empty safety check to avoid string manipulation errors
        if (string.IsNullOrWhiteSpace(location)) return "Can Tho";

        // Split address by comma to extract the province/city at the end
        var parts = location.Split(',');
        string cityCandidate = parts.Length > 0 ? parts[^1].Trim() : location.Trim();
        
        // If commas result in an empty candidate, assign default
        if (string.IsNullOrWhiteSpace(cityCandidate))
        {
            cityCandidate = "Can Tho";
        }

        // Clean common administrative prefixes in Vietnam
        cityCandidate = cityCandidate
            .Replace("TP.", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Thành phố", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Tỉnh", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

        // Normalize to unsigned English for accurate recognition by wttr.in API
        if (cityCandidate.Contains("Hồ Chí Minh", StringComparison.OrdinalIgnoreCase) || 
            cityCandidate.Contains("HCM", StringComparison.OrdinalIgnoreCase) ||
            location.Contains("Nguyễn Văn Cừ", StringComparison.OrdinalIgnoreCase) ||
            location.Contains("Võ Văn Ngân", StringComparison.OrdinalIgnoreCase))
        {
            return "Ho Chi Minh";
        }

        if (cityCandidate.Contains("Hà Nội", StringComparison.OrdinalIgnoreCase) || 
            cityCandidate.Contains("Hanoi", StringComparison.OrdinalIgnoreCase))
        {
            return "Hanoi";
        }

        if (cityCandidate.Contains("Cần Thơ", StringComparison.OrdinalIgnoreCase) || 
            cityCandidate.Contains("Can Tho", StringComparison.OrdinalIgnoreCase) ||
            cityCandidate.Contains("CT", StringComparison.OrdinalIgnoreCase))
        {
            return "Can Tho";
        }

        if (cityCandidate.Contains("Đà Nẵng", StringComparison.OrdinalIgnoreCase) || 
            cityCandidate.Contains("Da Nang", StringComparison.OrdinalIgnoreCase))
        {
            return "Da Nang";
        }

        return cityCandidate;
    }

    private static string TranslateCondition(string condition)
    {
        condition = condition.ToLower();
        if (condition.Contains("sunny") || condition.Contains("clear")) return "Nắng ráo";
        if (condition.Contains("cloudy") || condition.Contains("overcast")) return "Nhiều mây";
        if (condition.Contains("rain") || condition.Contains("shower")) return "Có mưa";
        if (condition.Contains("thunder") || condition.Contains("storm")) return "Dông bão";
        if (condition.Contains("mist") || condition.Contains("fog")) return "Có sương mù";
        return "Nhiệt đới";
    }

    private static string MapConditionToIcon(string condition)
    {
        condition = condition.ToLower();
        if (condition.Contains("sunny") || condition.Contains("clear")) return "bi-sun-fill text-warning";
        if (condition.Contains("cloudy") || condition.Contains("overcast")) return "bi-cloudy-fill text-secondary";
        if (condition.Contains("rain") || condition.Contains("shower")) return "bi-cloud-rain-heavy text-primary";
        if (condition.Contains("thunder") || condition.Contains("storm")) return "bi-cloud-lightning-rain text-danger";
        return "bi-cloud-sun text-warning";
    }

    private static WeatherDTO GetFallbackWeather(string location)
    {
        // Slightly randomize temperature for visual variance
        var hour = DateTime.Now.Hour;
        double baseTemp = (hour > 18 || hour < 6) ? 26.5 : 31.0;
        baseTemp += new Random().Next(-2, 3);

        return new WeatherDTO
        {
            Location = location,
            Temperature = baseTemp,
            Condition = "Nắng ráo",
            WindSpeed = 12.5,
            Humidity = 75,
            WeatherIconClass = "bi-sun-fill text-warning",
            FetchedAt = DateTime.UtcNow
        };
    }
}
