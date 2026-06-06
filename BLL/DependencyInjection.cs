using Microsoft.Extensions.DependencyInjection;

namespace BLL;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services)
    {
        // Đăng ký các Interface/Service của tầng BLL tại đây
        
        return services;
    }
}
