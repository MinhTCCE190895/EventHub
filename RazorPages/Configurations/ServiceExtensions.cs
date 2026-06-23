using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;
using DAL.Data;

namespace RazerPages.Configurations;

public static class ServiceExtensions
{
    public static void AddCustomAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = ".EventHub.Auth";
                // options.Cookie.Domain = ".unievent.edu.vn"; // Chỉ bật khi deploy thực tế lên subdomain, KHÔNG dùng ở localhost
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
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdminRole",        policy => policy.RequireRole("Admin"));
            options.AddPolicy("RequireOrganizerRole",    policy => policy.RequireRole("Organizer"));
            options.AddPolicy("AdminOrOrganizer",        policy => policy.RequireRole("Admin", "Organizer"));
            options.AddPolicy("RequireStudentRole",      policy => policy.RequireRole("Student"));
        });

        services.AddRazorPages(options =>
        {
            // Tất cả page đều cần đăng nhập (trừ các exception bên dưới)
            options.Conventions.AuthorizeFolder("/");

            // Public pages — không cần đăng nhập
            options.Conventions.AllowAnonymousToPage("/Index");
            options.Conventions.AllowAnonymousToPage("/Error");
            options.Conventions.AllowAnonymousToPage("/Privacy");

            // CRUD management — Admin và Organizer
            // Lưu ý: /Events KHÔNG dùng folder-level policy vì Feedback & MyFeedbacks
            // là Student-only pages nằm trong cùng folder. Phân quyền Events được
            // xử lý trực tiếp bằng [Authorize(Roles=...)] ở từng PageModel.
            options.Conventions.AuthorizeFolder("/Categories", "AdminOrOrganizer");
            options.Conventions.AuthorizeFolder("/Tags",       "AdminOrOrganizer");
            options.Conventions.AuthorizeFolder("/Venues",     "AdminOrOrganizer");

            // Student-only pages
            options.Conventions.AuthorizePage("/Events/Feedback",    "RequireStudentRole");
            options.Conventions.AuthorizePage("/Events/MyFeedbacks", "RequireStudentRole");
        });
    }

    public static void AddCustomDataProtection(this IServiceCollection services)
    {
        services.AddDataProtection()
            .PersistKeysToDbContext<AppDbContext>()
            .SetApplicationName("EventHub");
    }
}
