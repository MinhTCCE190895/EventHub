using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Authorization;
using DAL.Data;

namespace MVC.Configurations;

public static class ServiceExtensions
{
    public static void AddCustomAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = ".EventHub.Auth";
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
            });
    }

    public static void AddCustomAuthorization(this IServiceCollection services)
    {
        services.AddControllersWithViews(options =>
        {
            // Yêu cầu đăng nhập cho tất cả Controller mặc định
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.Filters.Add(new AuthorizeFilter(policy));
        });

        // Named policies — dùng [Authorize(Policy = "AdminOnly")] hoặc [Authorize(Roles = "Admin")]
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly",        p => p.RequireRole("Admin"));
            options.AddPolicy("OrganizerOnly",    p => p.RequireRole("Organizer"));
            options.AddPolicy("StudentOnly",      p => p.RequireRole("Student"));
            options.AddPolicy("AdminOrOrganizer", p => p.RequireRole("Admin", "Organizer"));
        });
    }

    public static void AddCustomDataProtection(this IServiceCollection services)
    {
        services.AddDataProtection()
            .PersistKeysToDbContext<AppDbContext>()
            .SetApplicationName("EventHub");
    }
}
