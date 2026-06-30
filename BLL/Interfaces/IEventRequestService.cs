using BusinessObjects.DTOs;

namespace BLL.Interfaces;

public interface IEventRequestService
{
    Task<IEnumerable<EventRequestDTO>> GetAllRequestsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<EventRequestDTO>> GetRequestsByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
    Task<EventRequestDTO?> GetRequestByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<EventRequestDTO> CreateRequestAsync(Guid studentId, EventRequestCreateDTO dto, CancellationToken cancellationToken = default);
    Task ProcessRequestAsync(EventRequestProcessDTO dto, CancellationToken cancellationToken = default);
}
