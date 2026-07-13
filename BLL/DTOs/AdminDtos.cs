namespace BLL.DTOs;

/// <summary>
/// Thống kê tổng quan cho Admin Dashboard
/// </summary>
public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int TotalEvents { get; set; }
    public int TotalBookings { get; set; }
    public int NewUsersLast7Days { get; set; }

    // 5 user mới nhất hiển thị nhanh trên Dashboard
    public List<UserListItemDto> RecentUsers { get; set; } = new();
}

/// <summary>
/// Thông tin 1 dòng user trong bảng quản lý
/// </summary>
public class UserListItemDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? StudentCode { get; set; }
    public string Role { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int EventCount { get; set; }
    public int BookingCount { get; set; }
}
