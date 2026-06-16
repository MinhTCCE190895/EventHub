using BLL.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.DataProtection;

namespace BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
    {
        // Cấu hình Shared DataProtection để chia sẻ Cookie Auth giữa MVC, RazorPages và Blazor
        services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(Path.Combine(Directory.GetCurrentDirectory(), "..", "SharedKeys")))
            .SetApplicationName("UniEventHub");

        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IBookmarkService, BookmarkService>();
        services.AddScoped<IBookingService, BookingService>();
        
        services.AddSignalR();
        services.AddScoped<IOrganizerService, OrganizerService>();
        services.AddScoped<IUserService, UserService>();

        services.AddAutoMapper(config => 
        {
            config.AddMaps(typeof(DependencyInjection).Assembly);
        });

        // Đăng ký dịch vụ thời tiết với HttpClient
        services.AddHttpClient<IWeatherService, WeatherService>();
        
        // Đăng ký MemoryCache để phục vụ lưu trữ đệm 30 phút theo yêu cầu
        services.AddMemoryCache();

        return services;
    }
}

