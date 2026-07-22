using System.Security.Claims;
using BLL;
using BLL.DTOs;
using BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC.ViewModels;

namespace MVC.Controllers;

[Authorize(Roles = ApplicationRoles.Admin)]
public class AdminController : Controller
{
    private const int UsersPageSize = 20;

    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var stats = await _adminService.GetDashboardStatsAsync(cancellationToken);
        return View(new AdminDashboardViewModel
        {
            TotalUsers = stats.TotalUsers,
            TotalEvents = stats.TotalEvents,
            TotalBookings = stats.TotalBookings,
            NewUsersLast7Days = stats.NewUsersLast7Days,
            RecentUsers = stats.RecentUsers.Select(AdminUserViewModel.FromDto).ToList()
        });
    }

    [HttpGet]
    public async Task<IActionResult> Users(
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        return View(await BuildUsersViewModelAsync(page, cancellationToken: cancellationToken));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleUserActive(
        Guid id,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetCurrentUserId(out var currentAdminId))
            return Forbid();

        var success = await _adminService.ToggleUserActiveAsync(
            id,
            currentAdminId,
            cancellationToken);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
            ? "Đã cập nhật trạng thái tài khoản thành công."
            : "Không thể thay đổi trạng thái tài khoản này.";
        return RedirectToAction(nameof(Users), new { page });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeUserRole(
        Guid id,
        string role,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetCurrentUserId(out var currentAdminId))
            return Forbid();

        var success = await _adminService.ChangeUserRoleAsync(
            id,
            role,
            currentAdminId,
            cancellationToken);
        TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
            ? "Đổi vai trò tài khoản thành công."
            : "Không thể đổi vai trò tài khoản này.";
        return RedirectToAction(nameof(Users), new { page });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(
        [Bind(Prefix = nameof(AdminUsersViewModel.NewUserForm))] AdminCreateUserViewModel model,
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetCurrentUserId(out var currentAdminId))
            return Forbid();

        if (!ModelState.IsValid)
            return View(nameof(Users), await BuildUsersViewModelAsync(
                page,
                model,
                true,
                cancellationToken));

        var dto = new AdminUserCreateDto
        {
            FullName = model.FullName,
            Email = model.Email,
            StudentCode = model.StudentCode,
            Password = model.Password,
            Role = model.Role
        };

        var result = await _adminService.CreateUserByAdminAsync(
            dto,
            currentAdminId,
            cancellationToken);
        if (!result.Success)
        {
            var field = string.IsNullOrWhiteSpace(result.ErrorField)
                ? string.Empty
                : $"{nameof(AdminUsersViewModel.NewUserForm)}.{result.ErrorField}";
            ModelState.AddModelError(field, result.ErrorMessage ?? "Không thể tạo tài khoản.");
            return View(nameof(Users), await BuildUsersViewModelAsync(
                page,
                model,
                true,
                cancellationToken));
        }

        TempData["SuccessMessage"] = $"Đã tạo thành công tài khoản {model.Email}.";
        return RedirectToAction(nameof(Users), new { page = 1 });
    }

    private bool TryGetCurrentUserId(out Guid userId)
    {
        return Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
    }

    private async Task<AdminUsersViewModel> BuildUsersViewModelAsync(
        int page,
        AdminCreateUserViewModel? newUserForm = null,
        bool openCreateUserModal = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _adminService.GetUsersAsync(page, UsersPageSize, cancellationToken);
        return new AdminUsersViewModel
        {
            Users = result.Items.Select(AdminUserViewModel.FromDto).ToList(),
            NewUserForm = newUserForm ?? new AdminCreateUserViewModel(),
            CurrentPage = result.PageNumber,
            PageSize = result.PageSize,
            TotalPages = result.TotalPages,
            TotalUsers = result.TotalCount,
            OpenCreateUserModal = openCreateUserModal
        };
    }
}
