using BusinessObjects.DTOs;
using DAL.Entities;

namespace BLL.Interfaces;

public interface IEventService
{
    Task<IEnumerable<EventDTO>> GetAllEventsAsync(CancellationToken cancellationToken = default);
    Task<EventDTO?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EventDTO> CreateEventAsync(EventCreateDTO dto, CancellationToken cancellationToken = default);
    Task UpdateEventAsync(EventUpdateDTO dto, CancellationToken cancellationToken = default);
    Task DeleteEventAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetOrganizersAsync(CancellationToken cancellationToken = default);
    Task ChangeEventStatusAsync(Guid id, string newStatus, CancellationToken cancellationToken = default);
}
