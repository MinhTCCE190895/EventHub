using System.Security.Claims;
using BLL;
using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using MVC.Configurations;
using MVC.ViewModels;

namespace MVC.Controllers;

public class AccountController : Controller
{
    private readonly IUserService _userService;
    private readonly ILogger<AccountController> _logger;
    private readonly PortalOptions _portalOptions;

    public AccountController(
        IUserService userService,
        ILogger<AccountController> logger,
        IOptions<PortalOptions> portalOptions)
    {
        _userService = userService;
        _logger = logger;
        _portalOptions = portalOptions.Value;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToDefaultDestination(User.FindFirstValue(ClaimTypes.Role));

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(MvcPolicies.AuthenticationRateLimit)]
    public async Task<IActionResult> Login(
        LoginViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var normalizedEmail = model.Email.Trim().ToLowerInvariant();
        var user = await _userService.ValidateLoginAsync(
            normalizedEmail,
            model.Password,
            cancellationToken);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng, hoặc tài khoản đã bị khóa.");
            return View(model);
        }

        var principal = UserPrincipalFactory.Create(user);
        var authenticationProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = model.RememberMe
                ? DateTimeOffset.UtcNow.AddDays(14)
                : DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authenticationProperties);
        _logger.LogInformation("User {UserId} logged in", user.Id);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl))
        {
            if (Url.IsLocalUrl(model.ReturnUrl))
                return LocalRedirect(model.ReturnUrl);

            if (IsAllowedPortalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);
        }

        return RedirectToDefaultDestination(user.Role);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToDefaultDestination(User.FindFirstValue(ClaimTypes.Role));

        return View(new RegisterViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(MvcPolicies.AuthenticationRateLimit)]
    public async Task<IActionResult> Register(
        RegisterViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _userService.RegisterStudentAsync(new StudentRegistrationDto
        {
            FullName = model.FullName,
            Email = model.Email,
            StudentCode = model.StudentCode,
            Password = model.Password,
            ConfirmPassword = model.ConfirmPassword
        }, cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(result.ErrorField ?? string.Empty, result.ErrorMessage ?? "Đăng ký thất bại.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
        return RedirectToAction(nameof(Login), new { returnUrl = model.ReturnUrl });
    }

    [HttpGet]
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        _logger.LogInformation("User {UserId} logged out", User.FindFirstValue(ClaimTypes.NameIdentifier));
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(MvcPolicies.AuthenticationRateLimit)]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var normalizedEmail = model.Email.Trim().ToLowerInvariant();
        var result = await _userService.SendPasswordResetOtpAsync(normalizedEmail, cancellationToken);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Không thể gửi mã OTP.");
            return View(model);
        }

        TempData["SuccessMessage"] =
            "Nếu tài khoản tồn tại, mã OTP đã được gửi. Vui lòng kiểm tra hộp thư.";
        return RedirectToAction(nameof(ResetPassword), new { email = normalizedEmail });
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ResetPassword(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return RedirectToAction(nameof(ForgotPassword));

        return View(new ResetPasswordViewModel { Email = email });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting(MvcPolicies.AuthenticationRateLimit)]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _userService.ResetPasswordWithOtpAsync(
            model.Email.Trim().ToLowerInvariant(),
            model.Otp,
            model.NewPassword,
            cancellationToken);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.ErrorMessage ?? "Không thể đặt lại mật khẩu.");
            return View(model);
        }

        TempData["SuccessMessage"] =
            "Đặt lại mật khẩu thành công! Vui lòng đăng nhập bằng mật khẩu mới.";
        return RedirectToAction(nameof(Login));
    }

    private IActionResult RedirectToDefaultDestination(string? role)
    {
        return role switch
        {
            ApplicationRoles.Admin => Redirect(_portalOptions.GetRazorPagesUrl()),
            ApplicationRoles.Organizer => Redirect(_portalOptions.GetRazorPagesUrl("Events")),
            ApplicationRoles.Student => Redirect(_portalOptions.GetRazorPagesUrl()),
            _ => RedirectToAction("Login", "Account")
        };
    }

    private bool IsAllowedPortalUrl(string returnUrl)
    {
        if (!Uri.TryCreate(returnUrl, UriKind.Absolute, out var candidate))
            return false;

        return HasSameOrigin(candidate, _portalOptions.RazorPagesBaseUrl) ||
               HasSameOrigin(candidate, _portalOptions.BlazorBaseUrl);
    }

    private static bool HasSameOrigin(Uri candidate, string configuredBaseUrl)
    {
        return Uri.TryCreate(configuredBaseUrl, UriKind.Absolute, out var allowed) &&
               string.Equals(candidate.Scheme, allowed.Scheme, StringComparison.OrdinalIgnoreCase) &&
               string.Equals(candidate.Host, allowed.Host, StringComparison.OrdinalIgnoreCase) &&
               candidate.Port == allowed.Port;
    }
}
