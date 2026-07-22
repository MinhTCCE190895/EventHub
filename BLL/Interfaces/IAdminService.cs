using BLL.DTOs;

namespace BLL.Interfaces;

public interface IAdminService
{
    Task<AdminDashboardDto> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
    Task<PagedResultDto<UserListItemDto>> GetUsersAsync(
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<bool> ToggleUserActiveAsync(
        Guid userId,
        Guid currentAdminId,
        CancellationToken cancellationToken = default);
    Task<bool> ChangeUserRoleAsync(
        Guid userId,
        string newRole,
        Guid currentAdminId,
        CancellationToken cancellationToken = default);
    Task<ServiceResultDto> CreateUserByAdminAsync(
        AdminUserCreateDto dto,
        Guid currentAdminId,
        CancellationToken cancellationToken = default);
}
