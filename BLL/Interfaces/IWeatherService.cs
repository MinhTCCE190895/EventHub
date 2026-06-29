using BusinessObjects.DTOs;

namespace BLL.Interfaces;

public interface IWeatherService
{
    // Lấy thông tin thời tiết dựa theo tên thành phố hoặc địa chỉ địa điểm
    Task<WeatherDTO?> GetWeatherAsync(string location);

    // Lấy dự báo thời tiết cho ngày cụ thể (hỗ trợ trong vòng 3 ngày tới)
    Task<WeatherDTO?> GetWeatherForecastAsync(string location, DateTime targetDate);
}

