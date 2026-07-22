using System.ComponentModel.DataAnnotations;
using BLL;
using BLL.DTOs;
using BLL.Interfaces;
using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class AdminService : IAdminService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdminService> _logger;

    public AdminService(AppDbContext context, ILogger<AdminService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<AdminDashboardDto> GetDashboardStatsAsync(
        CancellationToken cancellationToken = default)
    {
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        var users = _context.Users.AsNoTracking();

        var totalUsers = await users.CountAsync(cancellationToken);
        var totalEvents = await _context.Events.AsNoTracking().CountAsync(cancellationToken);
        var totalBookings = await _context.Bookings.AsNoTracking().CountAsync(cancellationToken);
        var newUsersLast7Days = await users.CountAsync(
            user => user.CreatedAt >= sevenDaysAgo,
            cancellationToken);
        var recentUsers = await ProjectUsers(users.OrderByDescending(user => user.CreatedAt))
            .Take(5)
            .ToListAsync(cancellationToken);

        return new AdminDashboardDto
        {
            TotalUsers = totalUsers,
            TotalEvents = totalEvents,
            TotalBookings = totalBookings,
            NewUsersLast7Days = newUsersLast7Days,
            RecentUsers = recentUsers
        };
    }

    public async Task<PagedResultDto<UserListItemDto>> GetUsersAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var normalizedPageSize = Math.Clamp(pageSize, 1, 100);
        var users = _context.Users.AsNoTracking();
        var totalCount = await users.CountAsync(cancellationToken);
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)normalizedPageSize));
        var normalizedPageNumber = Math.Clamp(pageNumber, 1, totalPages);

        var items = await ProjectUsers(users.OrderByDescending(user => user.CreatedAt))
            .Skip((normalizedPageNumber - 1) * normalizedPageSize)
            .Take(normalizedPageSize)
            .ToListAsync(cancellationToken);

        return new PagedResultDto<UserListItemDto>
        {
            Items = items,
            PageNumber = normalizedPageNumber,
            PageSize = normalizedPageSize,
            TotalCount = totalCount
        };
    }

    public async Task<bool> ToggleUserActiveAsync(
        Guid userId,
        Guid currentAdminId,
        CancellationToken cancellationToken = default)
    {
        if (userId == currentAdminId)
        {
            _logger.LogWarning("Admin {AdminId} attempted to lock their own account", currentAdminId);
            return false;
        }

        var user = await _context.Users.FindAsync([userId], cancellationToken);
        if (user is null || string.Equals(user.Role, ApplicationRoles.Admin, StringComparison.Ordinal))
            return false;

        user.IsActive = !user.IsActive;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Admin {AdminId} toggled User {UserId} active state to {IsActive}",
            currentAdminId,
            userId,
            user.IsActive);
        return true;
    }

    public async Task<bool> ChangeUserRoleAsync(
        Guid userId,
        string newRole,
        Guid currentAdminId,
        CancellationToken cancellationToken = default)
    {
        if (userId == currentAdminId || !ApplicationRoles.IsManageable(newRole))
            return false;

        var user = await _context.Users.FindAsync([userId], cancellationToken);
        if (user is null || string.Equals(user.Role, ApplicationRoles.Admin, StringComparison.Ordinal))
            return false;

        var oldRole = user.Role;
        user.Role = newRole;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Admin {AdminId} changed User {UserId} role from {OldRole} to {NewRole}",
            currentAdminId,
            userId,
            oldRole,
            newRole);
        return true;
    }

    public async Task<ServiceResultDto> CreateUserByAdminAsync(
        AdminUserCreateDto dto,
        Guid currentAdminId,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(dto.FullName))
            return ServiceResultDto.Failed("Họ tên là bắt buộc.", "FullName");

        if (!new EmailAddressAttribute().IsValid(normalizedEmail))
            return ServiceResultDto.Failed("Email không hợp lệ.", "Email");

        if (dto.Password.Length < 6)
            return ServiceResultDto.Failed("Mật khẩu phải có ít nhất 6 ký tự.", "Password");

        if (!ApplicationRoles.IsManageable(dto.Role))
            return ServiceResultDto.Failed("Vai trò không hợp lệ.", "Role");

        if (dto.Role == ApplicationRoles.Student && string.IsNullOrWhiteSpace(dto.StudentCode))
        {
            return ServiceResultDto.Failed(
                "Mã số sinh viên là bắt buộc đối với tài khoản Sinh viên.",
                "StudentCode");
        }

        if (await _context.Users.AsNoTracking()
                .AnyAsync(user => user.Email == normalizedEmail, cancellationToken))
        {
            return ServiceResultDto.Failed("Email này đã được sử dụng.", "Email");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName.Trim(),
            Email = normalizedEmail,
            StudentCode = string.IsNullOrWhiteSpace(dto.StudentCode) ? null : dto.StudentCode.Trim(),
            Role = dto.Role,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Admin {AdminId} created User {UserId} with role {Role}",
            currentAdminId,
            user.Id,
            user.Role);
        return ServiceResultDto.Succeeded();
    }

    private static IQueryable<UserListItemDto> ProjectUsers(IQueryable<User> users)
    {
        return users.Select(user => new UserListItemDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            StudentCode = user.StudentCode,
            Role = user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            EventCount = user.OrganizedEvents.Count,
            BookingCount = user.Bookings.Count
        });
    }
}
