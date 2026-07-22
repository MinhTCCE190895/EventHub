namespace BLL.DTOs;

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int TotalEvents { get; set; }
    public int TotalBookings { get; set; }
    public int NewUsersLast7Days { get; set; }
    public List<UserListItemDto> RecentUsers { get; set; } = new();
}

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

public class PagedResultDto<T>
{
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => TotalCount == 0
        ? 1
        : (int)Math.Ceiling(TotalCount / (double)PageSize);
}
