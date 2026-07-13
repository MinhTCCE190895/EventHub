using BLL.DTOs;

namespace MVC.ViewModels;

/// <summary>
/// ViewModel cho trang Admin Dashboard (Index)
/// </summary>
public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalEvents { get; set; }
    public int TotalBookings { get; set; }
    public int NewUsersLast7Days { get; set; }
    public List<UserListItemDto> RecentUsers { get; set; } = new();
}

/// <summary>
/// ViewModel cho trang Admin User Management (Users)
/// </summary>
public class AdminUsersViewModel
{
    public List<UserListItemDto> Users { get; set; } = new();
}
