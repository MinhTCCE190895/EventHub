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

    // Lấy thời tiết hiện tại của địa điểm
    public async Task<WeatherDTO?> GetWeatherAsync(string? location)
    {
        if (string.IsNullOrWhiteSpace(location)) return null;
        
        string normalized = NormalizeLocation(location);
        string cacheKey = $"weather_{normalized.ToLower().Replace(" ", "_")}";

        // Bước 1: Kiểm tra xem thời tiết của địa điểm này đã có trong cache chưa
        if (_cache.TryGetValue(cacheKey, out WeatherDTO? cachedWeather))
        {
            return cachedWeather;
        }

        try
        {
            // Thiết lập timeout ngắn 3 giây để tránh làm đơ trang web nếu API bên thứ ba bị chậm
            _httpClient.Timeout = TimeSpan.FromSeconds(3);
            string url = $"https://wttr.in/{Uri.EscapeDataString(normalized)}?format=j1";
            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(jsonString);
                var root = doc.RootElement;
                
                // Đọc thông tin thời tiết hiện tại từ JSON
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

                // Lưu dữ liệu vào cache trong 30 phút để tránh gọi API quá nhiều
                _cache.Set(cacheKey, weather, TimeSpan.FromMinutes(30));
                return weather;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi gọi API thời tiết cho {Location}. Sẽ dùng dữ liệu dự phòng.", location);
        }

        // Nếu API lỗi hoặc mất mạng, trả về dữ liệu thời tiết mặc định (fallback)
        var fallbackWeather = GetFallbackWeather(normalized);
        _cache.Set(cacheKey, fallbackWeather, TimeSpan.FromMinutes(5));
        return fallbackWeather;
    }

    // Lấy dự báo thời tiết cho ngày và giờ diễn ra sự kiện
    public async Task<WeatherDTO?> GetWeatherForecastAsync(string? location, DateTime targetDate)
    {
        if (string.IsNullOrWhiteSpace(location)) return null;

        // Tính khoảng cách ngày (chuyển sang múi giờ Việt Nam UTC+7 để tính toán chính xác)
        var today = DateTime.UtcNow.AddHours(7).Date;
        var targetDateLocal = targetDate.Date;
        var daysDifference = (targetDateLocal - today).Days;

        // Chỉ hỗ trợ dự báo thời tiết trong khoảng 3 ngày (Hôm nay, ngày mai, ngày kia)
        if (daysDifference < 0 || daysDifference > 2)
        {
            return null;
        }

        string normalized = NormalizeLocation(location);
        string cacheKey = $"weather_fc_day_{normalized.ToLower().Replace(" ", "_")}_{targetDateLocal:yyyyMMdd}";

        // Thử lấy danh sách dự báo thời tiết 8 khung giờ trong ngày từ cache
        if (_cache.TryGetValue(cacheKey, out List<WeatherDTO>? cachedForecast) && cachedForecast != null && cachedForecast.Count > 0)
        {
            int index = GetClosestHourlyIndex(targetDate, cachedForecast.Count - 1);
            return cachedForecast[index];
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
                    List<WeatherDTO>? targetResultList = null;
                    
                    // Duyệt qua các ngày dự báo trong kết quả API để lưu cache cho từng ngày
                    foreach (var day in weatherList.EnumerateArray())
                    {
                        if (day.TryGetProperty("date", out var dateProp) && 
                            DateTime.TryParse(dateProp.GetString(), out var parsedDate))
                        {
                            var dateKey = parsedDate.Date;
                            var hourlyArray = day.GetProperty("hourly");
                            var dayForecastList = new List<WeatherDTO>();

                            // Duyệt qua 8 khung giờ trong ngày (mỗi khung cách nhau 3 tiếng từ wttr.in)
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

                            // Lưu danh sách dự báo 8 khung giờ của ngày này vào cache trong 30 phút
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

        // Tạo dữ liệu dự phòng gồm 8 slot giống nhau nếu API bị lỗi
        var fallbackWeather = GetFallbackWeather(normalized);
        var fallbackList = Enumerable.Repeat(fallbackWeather, 8).ToList();
        _cache.Set(cacheKey, fallbackList, TimeSpan.FromMinutes(5));
        
        int fallbackIndex = GetClosestHourlyIndex(targetDate, 7);
        return fallbackList[fallbackIndex];
    }

    // Tìm khung giờ dự báo gần nhất với giờ bắt đầu sự kiện (wttr.in cung cấp 8 mốc: 0h, 3h, 6h, 9h, 12h, 15h, 18h, 21h)
    private static int GetClosestHourlyIndex(DateTime targetDate, int maxIndex)
    {
        int hour = targetDate.Hour;
        int index = 0;

        // Phân chia khung giờ đơn giản, tường minh cho đồ án
        if (hour >= 2 && hour < 5) index = 1;       // 03:00
        else if (hour >= 5 && hour < 8) index = 2;  // 06:00
        else if (hour >= 8 && hour < 11) index = 3; // 09:00
        else if (hour >= 11 && hour < 14) index = 4;// 12:00
        else if (hour >= 14 && hour < 17) index = 5;// 15:00
        else if (hour >= 17 && hour < 20) index = 6;// 18:00
        else if (hour >= 20 || hour < 2) index = 7; // 21:00

        return Math.Clamp(index, 0, maxIndex);
    }

    // Chuẩn hóa địa điểm từ địa chỉ Venue (lấy Tỉnh/Thành phố ở cuối chuỗi để API wttr.in nhận dạng tốt hơn)
    private static string NormalizeLocation(string location)
    {
        if (string.IsNullOrWhiteSpace(location)) return "Can Tho";

        // Tách địa chỉ bằng dấu phẩy để lấy phần cuối cùng (thường là Tỉnh/Thành phố)
        var parts = location.Split(',');
        string cityCandidate = parts.Length > 0 ? parts[^1].Trim() : location.Trim();
        
        if (string.IsNullOrWhiteSpace(cityCandidate))
        {
            cityCandidate = "Can Tho";
        }

        // Loại bỏ các tiền tố hành chính phổ biến ở Việt Nam
        cityCandidate = cityCandidate
            .Replace("TP.", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Thành phố", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Tỉnh", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

        // Ánh xạ nhanh về tên tiếng Anh không dấu cho API dễ nhận dạng
        if (cityCandidate.Contains("Hồ Chí Minh", StringComparison.OrdinalIgnoreCase) || 
            cityCandidate.Contains("HCM", StringComparison.OrdinalIgnoreCase))
        {
            return "Ho Chi Minh";
        }

        if (cityCandidate.Contains("Hà Nội", StringComparison.OrdinalIgnoreCase) || 
            cityCandidate.Contains("Hanoi", StringComparison.OrdinalIgnoreCase))
        {
            return "Hanoi";
        }

        if (cityCandidate.Contains("Cần Thơ", StringComparison.OrdinalIgnoreCase) || 
            cityCandidate.Contains("Can Tho", StringComparison.OrdinalIgnoreCase))
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

    // Dịch mô tả thời tiết tiếng Anh từ wttr.in sang tiếng Việt cho thân thiện
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

    // Ánh xạ trạng thái thời tiết sang CSS class của Bootstrap Icon
    private static string MapConditionToIcon(string condition)
    {
        condition = condition.ToLower();
        if (condition.Contains("sunny") || condition.Contains("clear")) return "bi-sun-fill text-warning";
        if (condition.Contains("cloudy") || condition.Contains("overcast")) return "bi-cloudy-fill text-secondary";
        if (condition.Contains("rain") || condition.Contains("shower")) return "bi-cloud-rain-heavy text-primary";
        if (condition.Contains("thunder") || condition.Contains("storm")) return "bi-cloud-lightning-rain text-danger";
        return "bi-cloud-sun text-warning";
    }

    // Dữ liệu dự phòng mặc định khi API gặp sự cố
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
