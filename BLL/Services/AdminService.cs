using BLL.DTOs;
using DAL.Data;
using DAL.Entities;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class AdminService : IAdminService
{
    private readonly IRepository<User> _userRepo;
    private readonly AppDbContext _context;
    private readonly ILogger<AdminService> _logger;

    public AdminService(IRepository<User> userRepo, AppDbContext context, ILogger<AdminService> logger)
    {
        _userRepo = userRepo;
        _context = context;
        _logger = logger;
    }

    public async Task<AdminDashboardDto> GetDashboardStatsAsync()
    {
        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);

        // Lấy thống kê tổng quan bằng các query riêng biệt, tránh JOIN phức tạp
        var totalUsers = await _context.Users.CountAsync();
        var totalEvents = await _context.Events.CountAsync();
        var totalBookings = await _context.Bookings.CountAsync();
        var newUsers7Days = await _context.Users.CountAsync(u => u.CreatedAt >= sevenDaysAgo);

        // 5 user mới nhất cho Dashboard quick view
        var recentUsers = await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .Take(5)
            .Select(u => new UserListItemDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                StudentCode = u.StudentCode,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                EventCount = u.OrganizedEvents.Count,
                BookingCount = u.Bookings.Count
            })
            .ToListAsync();

        return new AdminDashboardDto
        {
            TotalUsers = totalUsers,
            TotalEvents = totalEvents,
            TotalBookings = totalBookings,
            NewUsersLast7Days = newUsers7Days,
            RecentUsers = recentUsers
        };
    }

    public async Task<IEnumerable<UserListItemDto>> GetAllUsersAsync()
    {
        // Projection trực tiếp sang DTO để tối ưu query — chỉ SELECT cột cần thiết
        return await _context.Users
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new UserListItemDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                StudentCode = u.StudentCode,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                EventCount = u.OrganizedEvents.Count,
                BookingCount = u.Bookings.Count
            })
            .ToListAsync();
    }

    public async Task<bool> ToggleUserActiveAsync(Guid userId, Guid currentAdminId)
    {
        // Không cho Admin khóa chính mình
        if (userId == currentAdminId)
        {
            _logger.LogWarning("Admin {AdminId} attempted to lock own account", currentAdminId);
            return false;
        }

        var user = await _userRepo.GetByIdAsync(userId);

        if (user is null)
        {
            _logger.LogWarning("ToggleUserActive failed — User {UserId} not found", userId);
            return false;
        }

        // Chặn khóa tài khoản Admin khác — chỉ cho khóa Student và Organizer
        if (user.Role == "Admin")
        {
            _logger.LogWarning("Admin {AdminId} attempted to lock another Admin {UserId}", currentAdminId, userId);
            return false;
        }

        // Toggle trạng thái IsActive
        user.IsActive = !user.IsActive;
        _userRepo.Update(user);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Admin {AdminId} toggled User {UserId} ({Email}) IsActive → {IsActive}",
            currentAdminId, userId, user.Email, user.IsActive);

        return true;
    }
}
