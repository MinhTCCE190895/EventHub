using BLL.Interfaces;
using BLL.Services;
using BLL.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSignalR();

        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IBookmarkService, BookmarkService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IVenueService, VenueService>();
        services.AddScoped<IEventRequestService, EventRequestService>();

        services.AddAutoMapper(config => 
        {
            config.AddMaps(typeof(DependencyInjection).Assembly);
        });

        // Đăng ký dịch vụ thời tiết với HttpClient
        services.AddHttpClient<IWeatherService, WeatherService>();
        
        // Đăng ký MemoryCache để phục vụ lưu trữ đệm 30 phút theo yêu cầu
        services.AddMemoryCache();

        // Bind EmailSettings từ appsettings.json
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

        // Đăng ký các Interface/Service của tầng BLL tại đây
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<IEventReminderService, EventReminderService>();
        services.AddScoped<IFeedbackAnalyticsService, FeedbackAnalyticsService>();
        services.AddScoped<IFollowService, FollowService>();

        return services;
    }
}

