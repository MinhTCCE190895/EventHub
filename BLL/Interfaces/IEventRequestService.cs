using BLL.DTOs;

namespace BLL.Interfaces;

public interface IEventRequestService
{
    Task<IEnumerable<EventRequestDTO>> GetAllRequestsAsync(string? statusFilter = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<EventRequestDTO>> GetRequestsByStudentIdAsync(Guid studentId, string? statusFilter = null, CancellationToken cancellationToken = default);
    Task<EventRequestDTO?> GetRequestByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<EventRequestDTO> CreateRequestAsync(Guid studentId, EventRequestCreateDTO dto, CancellationToken cancellationToken = default);
    Task ProcessRequestAsync(EventRequestProcessDTO dto, CancellationToken cancellationToken = default);
}
