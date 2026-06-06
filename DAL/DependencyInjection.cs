using DAL.Data;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DAL;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("EventHub") 
                ?? throw new InvalidOperationException("Connection string 'EventHub' not found.")));

        // Đăng ký generic repository
        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

        return services;
    }
}
