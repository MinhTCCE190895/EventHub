using BLL.DTOs;
using BLL.Services;
using BLL.Interfaces;
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
            return Redirect("http://localhost:5129/");

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    // POST /Account/Login
    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken] // Ch?ng t?n công CSRF (Cross-Site Request Forgery) b?ng cách yêu c?u token h?p l? ?n trong form dang nh?p.
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // 1. Ki?m tra tính h?p l? c?a ViewModel (Data Annotations) d? l?c s?m d? li?u sai d?nh d?ng.
        if (!ModelState.IsValid)
            return View(model);

        // 2. G?i logic BLL d? xác th?c tài kho?n và ki?m tra m?t kh?u dã mã hóa.
        var user = await _userService.ValidateLoginAsync(model.Email, model.Password);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Email ho?c m?t kh?u không dúng, ho?c tài kho?n dã b? khoá.");
            return View(model);
        }

        // 3. Kh?i t?o danh sách Claims d? c?p phát cho Cookie, ch?a thông tin d?nh danh và phân quy?n.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role)
        };

        // 4. Thi?t l?p AuthenticationCookie và th?i gian s?ng (Session vs Persistent tùy thu?c vào RememberMe)
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var authProps = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = model.RememberMe
                ? DateTimeOffset.UtcNow.AddDays(14)
                : DateTimeOffset.UtcNow.AddHours(8)
        };

        // 5. SignIn vào Context d? sinh Cookie auth g?i v? client
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);
        _logger.LogInformation("User {Email} logged in", user.Email);

        // Ch?ng Open Redirect. Ch? cho phép d?nh tuy?n n?i b? ho?c các subdomain SSO h?p l?.
        if (!string.IsNullOrEmpty(model.ReturnUrl) && 
           (Url.IsLocalUrl(model.ReturnUrl) || 
            model.ReturnUrl.StartsWith("http://localhost:5129") || 
            model.ReturnUrl.StartsWith("https://localhost:7129") ||
            model.ReturnUrl.StartsWith("https://localhost:7170")))
        {
            return Redirect(model.ReturnUrl);
        }

        // Ði?u hu?ng ngu?i dùng v? các phân h? tuong ?ng theo phân quy?n (RBAC).
        return user.Role switch
        {
            "Organizer" => Redirect("http://localhost:5129/Events"),
            "Admin" => RedirectToAction("Index", "Admin"),
            "Student" => Redirect("http://localhost:5129/"),
            _ => RedirectToAction("Index", "Home")
        };
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
    [ValidateAntiForgeryToken] // Yêu c?u ValidateAntiForgeryToken d? tránh vi?c submit form gi? m?o t? trang khác.
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        // 1. Ki?m tra nhanh d?nh d?ng d? li?u d?u vào (d? dài, ký t? h?p l?).
        if (!ModelState.IsValid)
            return View(model);

        // 2. Ch?n tiêm quy?n Admin qua form dang ký công khai (dây là l? h?ng b?o m?t n?u b? sót).
        if (model.Role == "Admin")
        {
            ModelState.AddModelError("Role", "Không th? dang ký tài kho?n Admin.");
            return View(model);
        }

        // 3. Xác minh không b? trùng l?p tài kho?n (Business logic validator).
        if (await _userService.EmailExistsAsync(model.Email))
        {
            ModelState.AddModelError("Email", "Email này dã du?c s? d?ng.");
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

        TempData["SuccessMessage"] = "Ðang ký thành công! Vui lòng dang nh?p.";
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
        return RedirectToAction(nameof(Login));
    }

    // GET /Account/AccessDenied
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
