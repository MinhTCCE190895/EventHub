using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC.ViewModels;
using System.Security.Claims;

namespace MVC.Controllers;

public class AccountController : Controller
{
    private readonly IUserService _userService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IUserService userService, ILogger<AccountController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    // GET /Account/Login
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // POST /Account/Login
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userService.ValidateLoginAsync(model.Email, model.Password);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng, hoặc tài khoản đã bị khoá.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var authProps = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = model.RememberMe
                ? DateTimeOffset.UtcNow.AddDays(14)
                : DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);
        _logger.LogInformation("User {Email} logged in", user.Email);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && 
           (Url.IsLocalUrl(model.ReturnUrl) || 
            model.ReturnUrl.StartsWith("http://localhost:5129") || 
            model.ReturnUrl.StartsWith("https://localhost:7129") ||
            model.ReturnUrl.StartsWith("https://localhost:7170")))
        {
            return Redirect(model.ReturnUrl);
        }

        // Redirect theo role
        if (user.Role == "Admin")
        {
            return Redirect("http://localhost:5129/");
        }

        if (user.Role == "Organizer")
        {
            // Organizer quản lý sự kiện nên redirect thẳng vào trang Events
            return Redirect("http://localhost:5129/Events");
        }

        if (user.Role == "Student")
        {
            // Student chuyển về RazorPages (local 5129)
            return Redirect("http://localhost:5129/");
        }

        return RedirectToAction("Index", "Home");
    }

    // GET /Account/Register
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new RegisterViewModel());
    }

    // POST /Account/Register
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Chặn role Admin được tạo qua form
        if (model.Role == "Admin")
        {
            ModelState.AddModelError("Role", "Không thể đăng ký tài khoản Admin.");
            return View(model);
        }

        if (await _userService.EmailExistsAsync(model.Email))
        {
            ModelState.AddModelError("Email", "Email này đã được sử dụng.");
            return View(model);
        }

        var dto = new RegisterDto
        {
            FullName = model.FullName,
            Email = model.Email,
            StudentCode = model.StudentCode,
            Password = model.Password,
            ConfirmPassword = model.ConfirmPassword,
            Role = model.Role
        };

        await _userService.RegisterAsync(dto);
        _logger.LogInformation("New user registered: {Email}", model.Email);

        TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
        return RedirectToAction(nameof(Login));
    }

    // GET & POST /Account/Logout
    [HttpGet]
    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Logout()
    {
        _logger.LogInformation("User {Name} logged out", User.Identity?.Name);
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Register));
    }

    // GET /Account/AccessDenied
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
