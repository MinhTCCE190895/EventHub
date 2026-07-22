using BLL.Interfaces;
using System.Text.Json;
using BLL.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class WeatherService(HttpClient httpClient, IMemoryCache cache, ILogger<WeatherService> logger) : IWeatherService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly IMemoryCache _cache = cache;
    private readonly ILogger<WeatherService> _logger = logger;

    // Get the current weather of a location
    public async Task<WeatherDTO?> GetWeatherAsync(string? location)
    {
        if (string.IsNullOrWhiteSpace(location)) return null;
        
        string normalized = NormalizeLocation(location);
        string cacheKey = $"weather_{normalized.ToLower().Replace(" ", "_")}";

        if (_cache.TryGetValue(cacheKey, out WeatherDTO? cachedWeather))
        {
            return cachedWeather;
        }

        try
        {
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

                _cache.Set(cacheKey, weather, TimeSpan.FromMinutes(30));
                return weather;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gọi API thời tiết cho {Location}.", location);
        }

        var fallbackWeather = GetFallbackWeather(normalized);
        _cache.Set(cacheKey, fallbackWeather, TimeSpan.FromMinutes(5));
        return fallbackWeather;
    }

    // Fetch weather forecast for the event date and time
    public async Task<WeatherDTO?> GetWeatherForecastAsync(string? location, DateTime targetDate)
    {
        if (string.IsNullOrWhiteSpace(location)) return null;

        var today = DateTime.UtcNow.AddHours(7).Date;
        var targetDateLocal = targetDate.Date;
        var daysDifference = (targetDateLocal - today).Days;

        // Only support weather forecast within 3 days (today, tomorrow, next day)
        if (daysDifference < 0 || daysDifference > 2)
        {
            return null;
        }

        string normalized = NormalizeLocation(location);
        string cacheKey = $"weather_fc_day_{normalized.ToLower().Replace(" ", "_")}_{targetDateLocal:yyyyMMdd}";

        if (_cache.TryGetValue(cacheKey, out List<WeatherDTO>? cachedForecast) && cachedForecast != null)
        {
            return cachedForecast[GetClosestHourlyIndex(targetDate, cachedForecast.Count - 1)];
        }

        try
        {
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
                    // Access the day's forecast directly using daysDifference as the index
                    var day = weatherList[daysDifference];
                    var hourlyArray = day.GetProperty("hourly");
                    var dayForecastList = new List<WeatherDTO>();

                    foreach (var hourly in hourlyArray.EnumerateArray())
                    {
                        string desc = hourly.GetProperty("weatherDesc")[0].GetProperty("value").GetString() ?? "Trong lành";
                        dayForecastList.Add(new WeatherDTO
                        {
                            Location = normalized,
                            Temperature = double.Parse(hourly.GetProperty("tempC").GetString() ?? "28"),
                            Condition = TranslateCondition(desc),
                            WindSpeed = double.Parse(hourly.GetProperty("windspeedKmph").GetString() ?? "10"),
                            Humidity = int.Parse(hourly.GetProperty("humidity").GetString() ?? "75"),
                            WeatherIconClass = MapConditionToIcon(desc),
                            FetchedAt = DateTime.UtcNow
                        });
                    }

                    _cache.Set(cacheKey, dayForecastList, TimeSpan.FromMinutes(30));
                    return dayForecastList[GetClosestHourlyIndex(targetDate, dayForecastList.Count - 1)];
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gọi API dự báo thời tiết cho {Location} ngày {Date}.", location, targetDateLocal);
        }

        return GetFallbackWeather(normalized);
    }

    // Find the closest hourly forecast block to the event start time (wttr.in provides 8 slots)
    private static int GetClosestHourlyIndex(DateTime targetDate, int maxIndex)
    {
        int hour = targetDate.Hour;
        int index = 0;

        if (hour >= 2 && hour < 5) index = 1;       // 03:00
        else if (hour >= 5 && hour < 8) index = 2;  // 06:00
        else if (hour >= 8 && hour < 11) index = 3; // 09:00
        else if (hour >= 11 && hour < 14) index = 4;// 12:00
        else if (hour >= 14 && hour < 17) index = 5;// 15:00
        else if (hour >= 17 && hour < 20) index = 6;// 18:00
        else if (hour >= 20 || hour < 2) index = 7; // 21:00

        return Math.Clamp(index, 0, maxIndex);
    }

    // Normalize location from Venue address (extract city/province at the end for wttr.in recognition)
    private static string NormalizeLocation(string location)
    {
        if (string.IsNullOrWhiteSpace(location)) return "Can Tho";

        // Extract the last part of the address (usually the city/province)
        string city = location.Split(',').Last().Trim();

        // Map popular Vietnamese cities to English unsigned names for API compatibility
        if (city.Contains("Hồ Chí Minh", StringComparison.OrdinalIgnoreCase) || city.Contains("HCM", StringComparison.OrdinalIgnoreCase)) return "Ho Chi Minh";
        if (city.Contains("Hà Nội", StringComparison.OrdinalIgnoreCase) || city.Contains("Hanoi", StringComparison.OrdinalIgnoreCase)) return "Hanoi";
        if (city.Contains("Cần Thơ", StringComparison.OrdinalIgnoreCase) || city.Contains("Can Tho", StringComparison.OrdinalIgnoreCase)) return "Can Tho";
        if (city.Contains("Đà Nẵng", StringComparison.OrdinalIgnoreCase) || city.Contains("Da Nang", StringComparison.OrdinalIgnoreCase)) return "Da Nang";
        if (city.Contains("Quy Nhơn", StringComparison.OrdinalIgnoreCase) || city.Contains("Quy Nhon", StringComparison.OrdinalIgnoreCase)) return "Quy Nhon";

        // Remove common administrative prefixes
        return city
            .Replace("TP.", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Thành phố", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Tỉnh", "", StringComparison.OrdinalIgnoreCase)
            .Trim();
    }

    // Translate English weather description from wttr.in to friendly Vietnamese
    private static string TranslateCondition(string condition)
    {
        condition = condition.ToLower();
        if (condition.Contains("sunny") || condition.Contains("clear")) return "Nắng ráo";
        if (condition.Contains("cloudy") || condition.Contains("overcast")) return "Nhiều mây";
        if (condition.Contains("rain") || condition.Contains("shower")) return "Có mưa";
        if (condition.Contains("thunder") || condition.Contains("storm")) return "Dông bão";
        if (condition.Contains("mist") || condition.Contains("fog")) return "Có sương mù";
        return "Trong lành";
    }

    // Map weather condition to Bootstrap Icon CSS class
    private static string MapConditionToIcon(string condition)
    {
        condition = condition.ToLower();
        if (condition.Contains("sunny") || condition.Contains("clear")) return "bi-sun-fill text-warning";
        if (condition.Contains("cloudy") || condition.Contains("overcast")) return "bi-cloudy-fill text-secondary";
        if (condition.Contains("rain") || condition.Contains("shower")) return "bi-cloud-rain-heavy text-primary";
        if (condition.Contains("thunder") || condition.Contains("storm")) return "bi-cloud-lightning-rain text-danger";
        return "bi-cloud-sun text-warning";
    }

    // Default fallback weather when API encounters issues
    private static WeatherDTO GetFallbackWeather(string location)
    {
        return new WeatherDTO
        {
            Location = location,
            Temperature = 30.0,
            Condition = "Nắng ráo",
            WindSpeed = 12.0,
            Humidity = 75,
            WeatherIconClass = "bi-sun-fill text-warning",
            FetchedAt = DateTime.UtcNow
        };
    }
}
