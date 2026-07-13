using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC.ViewModels;
using System.Security.Claims;

namespace MVC.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminService adminService, ILogger<AdminController> logger)
    {
        _adminService = adminService;
        _logger = logger;
    }

    // GET /Admin — Dashboard thống kê
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var stats = await _adminService.GetDashboardStatsAsync();

        var viewModel = new AdminDashboardViewModel
        {
            TotalUsers = stats.TotalUsers,
            TotalEvents = stats.TotalEvents,
            TotalBookings = stats.TotalBookings,
            NewUsersLast7Days = stats.NewUsersLast7Days,
            RecentUsers = stats.RecentUsers
        };

        return View(viewModel);
    }

    // GET /Admin/Users — Bảng quản lý User
    [HttpGet]
    public async Task<IActionResult> Users()
    {
        var users = await _adminService.GetAllUsersAsync();

        var viewModel = new AdminUsersViewModel
        {
            Users = users.ToList()
        };

        return View(viewModel);
    }

    // POST /Admin/ToggleUserActive/{id} — Khóa/Mở khóa tài khoản
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserActive(Guid id)
    {
        // Lấy ID của Admin đang đăng nhập từ Claims
        var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(adminIdClaim, out var currentAdminId))
        {
            return Forbid();
        }

        var success = await _adminService.ToggleUserActiveAsync(id, currentAdminId);

        if (success)
        {
            TempData["SuccessMessage"] = "Đã cập nhật trạng thái tài khoản thành công.";
        }
        else
        {
            TempData["ErrorMessage"] = "Không thể thay đổi trạng thái tài khoản này.";
        }

        return RedirectToAction(nameof(Users));
    }
}
