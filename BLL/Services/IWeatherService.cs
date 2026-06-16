using BusinessObjects.DTOs;

namespace BLL.Services;

public interface IWeatherService
{
    // Lấy thông tin thời tiết dựa theo tên thành phố hoặc địa chỉ địa điểm
    Task<WeatherDTO?> GetWeatherAsync(string location);
}
