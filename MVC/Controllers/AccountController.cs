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
    [ValidateAntiForgeryToken] // Chống tấn công CSRF (Cross-Site Request Forgery) bằng cách yêu cầu token hợp lệ ẩn trong form đăng nhập.
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        // 1. Kiểm tra tính hợp lệ của ViewModel (Data Annotations) để lọc sớm dữ liệu sai định dạng.
        if (!ModelState.IsValid)
            return View(model);

        // 2. Gọi logic BLL để xác thực tài khoản và kiểm tra mật khẩu đã mã hóa.
        var user = await _userService.ValidateLoginAsync(model.Email, model.Password);

        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng, hoặc tài khoản đã bị khoá.");
            return View(model);
        }

        // 3. Khởi tạo danh sách Claims để cấp phát cho Cookie, chứa thông tin định danh và phân quyền.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role)
        };

        // 4. Thiết lập AuthenticationCookie và thời gian sống (Session vs Persistent tùy thuộc vào RememberMe)
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var authProps = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = model.RememberMe
                ? DateTimeOffset.UtcNow.AddDays(14)
                : DateTimeOffset.UtcNow.AddHours(8)
        };

        // 5. SignIn vào Context để sinh Cookie auth gửi về client
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);
        _logger.LogInformation("User {Email} logged in", user.Email);

        // Chống Open Redirect. Chỉ cho phép định tuyến nội bộ hoặc các subdomain SSO hợp lệ.
        if (!string.IsNullOrEmpty(model.ReturnUrl) && 
           (Url.IsLocalUrl(model.ReturnUrl) || 
            model.ReturnUrl.StartsWith("http://localhost:5129") || 
            model.ReturnUrl.StartsWith("https://localhost:7129") ||
            model.ReturnUrl.StartsWith("https://localhost:7170")))
        {
            return Redirect(model.ReturnUrl);
        }

        // Điều hướng người dùng về các phân hệ tương ứng theo phân quyền (RBAC).
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
    [ValidateAntiForgeryToken] // Yêu cầu ValidateAntiForgeryToken để tránh việc submit form giả mạo từ trang khác.
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        // 1. Kiểm tra nhanh định dạng dữ liệu đầu vào (độ dài, ký tự hợp lệ).
        if (!ModelState.IsValid)
            return View(model);

        // 2. Chặn tiêm quyền Admin qua form đăng ký công khai (đây là lỗ hổng bảo mật nếu bỏ sót).
        if (model.Role == "Admin")
        {
            ModelState.AddModelError("Role", "Không thể đăng ký tài khoản Admin.");
            return View(model);
        }

        // 3. Xác minh không bị trùng lặp tài khoản (Business logic validator).
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
        return RedirectToAction(nameof(Login));
    }

    // GET /Account/AccessDenied
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}