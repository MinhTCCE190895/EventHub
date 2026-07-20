using BLL.DTOs;

namespace BLL.Interfaces;

public interface IBookmarkService
{
    // Returns list of bookmarked EventIds — used on Index page to highlight bookmark toggle buttons
    Task<List<Guid>> GetBookmarkedEventIdsAsync(Guid studentId, CancellationToken ct = default);

    // Returns full event cards for all bookmarked events (used on /Bookmarks page)
    Task<List<EventCardDTO>> GetBookmarkedEventsAsync(Guid studentId, CancellationToken cancellationToken = default);

    // Checks whether a student has already bookmarked a given event
    Task<bool> IsBookmarkedAsync(Guid studentId, Guid eventId, CancellationToken cancellationToken = default);

    // Toggle: adds if not bookmarked, removes if already bookmarked. Returns true if now bookmarked.
    Task<bool> ToggleBookmarkAsync(Guid studentId, Guid eventId, CancellationToken cancellationToken = default);
}
