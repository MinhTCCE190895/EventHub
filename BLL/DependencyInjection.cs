using BLL.Services;
using BLL.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IBookmarkService, BookmarkService>();

        // Bind EmailSettings từ appsettings.json
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));

        // Đăng ký các Interface/Service của tầng BLL tại đây
        services.AddScoped<IEmailSender, EmailSender>();

        return services;
    }
}

