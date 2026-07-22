using System.ComponentModel.DataAnnotations;
using System.Globalization;
using BLL;
using BLL.DTOs;

namespace MVC.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalEvents { get; set; }
    public int TotalBookings { get; set; }
    public int NewUsersLast7Days { get; set; }
    public IReadOnlyList<AdminUserViewModel> RecentUsers { get; set; } = Array.Empty<AdminUserViewModel>();
}

public class AdminUsersViewModel
{
    public IReadOnlyList<AdminUserViewModel> Users { get; set; } = Array.Empty<AdminUserViewModel>();
    public AdminCreateUserViewModel NewUserForm { get; set; } = new();
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public int TotalUsers { get; set; }
    public bool OpenCreateUserModal { get; set; }
    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
    public IReadOnlyList<int> VisiblePages
    {
        get
        {
            var firstPage = Math.Max(1, CurrentPage - 2);
            var lastPage = Math.Min(TotalPages, firstPage + 4);
            firstPage = Math.Max(1, lastPage - 4);
            return Enumerable.Range(firstPage, lastPage - firstPage + 1).ToArray();
        }
    }
}

public class AdminUserViewModel
{
    private static readonly IReadOnlyDictionary<string, string> RoleBadgeClasses =
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [ApplicationRoles.Admin] = "bg-danger-subtle text-danger border-danger-subtle",
            [ApplicationRoles.Organizer] = "bg-primary-subtle text-primary border-primary-subtle"
        };

    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? StudentCode { get; set; }
    public string Role { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public int EventCount { get; set; }
    public int BookingCount { get; set; }

    public string RoleBadgeClass => RoleBadgeClasses.TryGetValue(Role, out var badgeClass)
        ? badgeClass
        : "bg-secondary-subtle text-secondary border-secondary-subtle";
    public string Initial => string.IsNullOrWhiteSpace(FullName)
        ? "U"
        : FullName[0].ToString().ToUpperInvariant();
    public bool CanBeManaged => !string.Equals(Role, ApplicationRoles.Admin, StringComparison.Ordinal);
    public string? NextRole => Role switch
    {
        ApplicationRoles.Student => ApplicationRoles.Organizer,
        ApplicationRoles.Organizer => ApplicationRoles.Student,
        _ => null
    };
    public string RoleChangeButtonClass => Role == ApplicationRoles.Student
        ? "btn-outline-primary"
        : "btn-outline-warning";
    public string RoleChangeIconClass => Role == ApplicationRoles.Student
        ? "bi-arrow-up-circle-fill"
        : "bi-arrow-down-circle-fill";
    public string RoleChangeTitle => Role == ApplicationRoles.Student
        ? "Cấp quyền Organizer"
        : "Hạ quyền xuống Student";
    public string RoleChangeConfirmation => Role == ApplicationRoles.Student
        ? $"Bạn có muốn cấp quyền Organizer cho {FullName}?"
        : $"Bạn có muốn hạ quyền của {FullName} xuống Student?";
    public string ActiveToggleButtonClass => IsActive ? "btn-outline-danger" : "btn-outline-success";
    public string ActiveToggleIconClass => IsActive ? "bi-lock-fill" : "bi-unlock-fill";
    public string ActiveToggleTitle => IsActive ? "Khóa tài khoản" : "Mở khóa tài khoản";
    public string ActiveToggleConfirmation => IsActive
        ? $"Bạn có chắc muốn khóa tài khoản {FullName}?"
        : $"Bạn có chắc muốn mở khóa tài khoản {FullName}?";
    public string CreatedAtUtcIso => DateTime.SpecifyKind(CreatedAt, DateTimeKind.Utc)
        .ToString("O", CultureInfo.InvariantCulture);
    public string CreatedAtUtcDisplay => $"{CreatedAt:dd/MM/yyyy HH:mm} UTC";

    public static AdminUserViewModel FromDto(UserListItemDto dto)
    {
        return new AdminUserViewModel
        {
            Id = dto.Id,
            FullName = dto.FullName,
            Email = dto.Email,
            StudentCode = dto.StudentCode,
            Role = dto.Role,
            IsActive = dto.IsActive,
            CreatedAt = dto.CreatedAt,
            EventCount = dto.EventCount,
            BookingCount = dto.BookingCount
        };
    }
}

public class AdminCreateUserViewModel
{
    [Required(ErrorMessage = "Họ tên là bắt buộc.")]
    [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    public string Email { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Mã số sinh viên tối đa 20 ký tự.")]
    public string? StudentCode { get; set; }

    [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vai trò là bắt buộc.")]
    [RegularExpression(ApplicationRoles.ManageableRolePattern, ErrorMessage = "Vai trò không hợp lệ.")]
    public string Role { get; set; } = ApplicationRoles.Student;
}
