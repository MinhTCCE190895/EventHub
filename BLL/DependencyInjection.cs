using BLL.Services;
using Microsoft.Extensions.DependencyInjection;


namespace BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
    {

        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IBookingService, BookingService>();
        services.AddSignalR();
        services.AddScoped<IOrganizerService, OrganizerService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IVenueService, VenueService>();

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

