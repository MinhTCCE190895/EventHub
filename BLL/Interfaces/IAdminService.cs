using BLL.DTOs;

namespace BLL.Interfaces;

public interface IAdminService
{
    /// <summary>
    /// Lấy thống kê tổng quan: tổng User, Event, Booking, user mới 7 ngày + 5 user gần nhất.
    /// </summary>
    Task<AdminDashboardDto> GetDashboardStatsAsync();

    /// <summary>
    /// Lấy danh sách tất cả user kèm số event đã tổ chức và số booking.
    /// </summary>
    Task<IEnumerable<UserListItemDto>> GetAllUsersAsync();

    /// <summary>
    /// Đổi trạng thái IsActive của user (true ↔ false).
    /// Trả về true nếu thành công, false nếu không tìm thấy hoặc không được phép (khóa Admin).
    /// </summary>
    Task<bool> ToggleUserActiveAsync(Guid userId, Guid currentAdminId);
}
