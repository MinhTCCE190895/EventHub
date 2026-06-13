using BusinessObjects.DTOs;

namespace BLL.Services;

public interface IBookmarkService
{
    // Toggle bookmark status (add if missing, remove if exists)
    Task<bool> ToggleBookmarkAsync(Guid studentId, Guid eventId, CancellationToken cancellationToken = default);

    // Get list of bookmarked events for a student
    Task<List<EventCardDTO>> GetBookmarkedEventsAsync(Guid studentId, CancellationToken cancellationToken = default);

    // Check if an event is bookmarked by a student
    Task<bool> IsBookmarkedAsync(Guid studentId, Guid eventId, CancellationToken cancellationToken = default);
}
