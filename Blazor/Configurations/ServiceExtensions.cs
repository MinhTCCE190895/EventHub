using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;
using DAL.Data;

namespace Blazor.Configurations;

public static class ServiceExtensions
{
    public static void AddCustomAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = ".EventHub.Auth";
                options.Cookie.Domain = ".unievent.edu.vn";
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                
                options.Events = new CookieAuthenticationEvents
                {
                    OnValidatePrincipal = async context =>
                    {
                        var userIdClaim = context.Principal?.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                        {
                            var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
                            var user = await dbContext.Users.FindAsync(userId);
                            if (user == null || !user.IsActive)
                            {
                                context.RejectPrincipal();
                                await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                            }
                        }
                    },
                    OnRedirectToLogin = context =>
                    {
                        var returnUrl = Uri.EscapeDataString(context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase + context.Request.Path + context.Request.QueryString);
                        context.Response.Redirect($"http://localhost:5259/Account/Login?ReturnUrl={returnUrl}");
                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = context =>
                    {
                        var returnUrl = Uri.EscapeDataString(context.Request.Scheme + "://" + context.Request.Host + context.Request.PathBase + context.Request.Path + context.Request.QueryString);
                        context.Response.Redirect($"http://localhost:5259/Account/AccessDenied?ReturnUrl={returnUrl}");
                        return Task.CompletedTask;
                    }
                };
            });
    }

    public static void AddCustomAuthorization(this IServiceCollection services)
    {
        // Yêu cầu đăng nhập cho tất cả các trang Blazor theo mặc định
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });
    }

    public static void AddCustomDataProtection(this IServiceCollection services)
    {
        services.AddDataProtection()
            .PersistKeysToDbContext<AppDbContext>()
            .SetApplicationName("EventHub");
    }
}
