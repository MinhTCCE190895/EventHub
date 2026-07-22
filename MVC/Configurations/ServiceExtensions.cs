using System.Security.Claims;
using System.Threading.RateLimiting;
using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace MVC.Configurations;

public static class ServiceExtensions
{
    private static readonly TimeSpan PrincipalValidationInterval = TimeSpan.FromMinutes(5);

    public static void AddCustomAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.Cookie.Name = ".EventHub.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.LoginPath = "/Account/Login";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromHours(8);
                options.SlidingExpiration = true;
                options.Events = new CookieAuthenticationEvents
                {
                    OnValidatePrincipal = ValidatePrincipalAsync
                };
            });
    }

    public static void AddMvcPresentation(this IServiceCollection services)
    {
        services.AddControllersWithViews(options =>
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.Filters.Add(new AuthorizeFilter(policy));
        });

        services.AddAuthorization();
    }

    public static void AddPortalOptions(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<PortalOptions>()
            .Bind(configuration.GetSection(PortalOptions.SectionName))
            .Validate(PortalOptions.HasValidUrls, "PortalUrls must contain valid HTTP or HTTPS URLs.")
            .ValidateOnStart();
    }

    public static void AddRequestRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(MvcPolicies.AuthenticationRateLimit, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    $"{context.Connection.RemoteIpAddress?.ToString() ?? "unknown"}:{context.Request.Path}",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(5),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
        });
    }

    private static async Task ValidatePrincipalAsync(CookieValidatePrincipalContext context)
    {
        var issuedUtc = context.Properties.IssuedUtc;
        if (issuedUtc.HasValue && DateTimeOffset.UtcNow - issuedUtc.Value < PrincipalValidationInterval)
            return;

        var userIdValue = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            await RejectPrincipalAsync(context);
            return;
        }

        var userService = context.HttpContext.RequestServices.GetRequiredService<IUserService>();
        var user = await userService.GetAuthenticatedUserAsync(
            userId,
            context.HttpContext.RequestAborted);
        if (user is null)
        {
            await RejectPrincipalAsync(context);
            return;
        }

        context.ReplacePrincipal(UserPrincipalFactory.Create(user));
        context.ShouldRenew = true;
    }

    private static async Task RejectPrincipalAsync(CookieValidatePrincipalContext context)
    {
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}

public static class MvcPolicies
{
    public const string AuthenticationRateLimit = "Authentication";
}

internal static class UserPrincipalFactory
{
    public static ClaimsPrincipal Create(AuthenticatedUserDto user)
    {
        var claims = new Claim[]
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);
        return new ClaimsPrincipal(identity);
    }
}
