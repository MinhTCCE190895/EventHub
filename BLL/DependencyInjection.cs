using BLL.Services;
using BLL.Interfaces;
using BLL.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCoreBusinessServices(configuration);
        services.AddUserManagementServices();
        services.AddEventManagementServices();
        services.AddFeedbackManagementServices();
        services.AddLiveInteractiveServices();

        return services;
    }

    public static IServiceCollection AddCoreBusinessServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAutoMapper(config => 
        {
            config.AddMaps(typeof(DependencyInjection).Assembly);
        });

        services.AddMemoryCache();
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.AddScoped<IEmailSender, EmailSender>();

        return services;
    }

    public static IServiceCollection AddUserManagementServices(this IServiceCollection services)
    {
        services.AddScoped<IUserService, UserService>();
        return services;
    }

    public static IServiceCollection AddEventManagementServices(this IServiceCollection services)
    {
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IVenueService, VenueService>();
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IFollowService, FollowService>();
        services.AddHttpClient<IWeatherService, WeatherService>();

        return services;
    }

    public static IServiceCollection AddFeedbackManagementServices(this IServiceCollection services)
    {
        services.AddScoped<IFeedbackAnalyticsService, FeedbackAnalyticsService>();
        return services;
    }

    public static IServiceCollection AddLiveInteractiveServices(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddScoped<IBookingService, BookingService>();
        services.AddScoped<IEventReminderService, EventReminderService>();

        return services;
    }
}

