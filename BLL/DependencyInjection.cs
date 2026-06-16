using BLL.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
    {
        services.AddScoped<ISearchService, SearchService>();
        services.AddScoped<IBookmarkService, BookmarkService>();
        services.AddScoped<IBookingService, BookingService>();

        return services;
    }
}
