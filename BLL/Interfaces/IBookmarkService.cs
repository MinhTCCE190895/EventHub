using BusinessObjects.DTOs;

namespace BLL.Interfaces;

public interface IBookmarkService
{
    // Trả về danh sách EventId đã bookmark, dùng trên Index để highlight nút bookmark
    Task<List<Guid>> GetBookmarkedEventIdsAsync(Guid studentId, CancellationToken ct = default);

    // Trả về danh sách event đã bookmark (dùng cho trang /Bookmarks)
    Task<List<EventCardDTO>> GetBookmarkedEventsAsync(Guid studentId, CancellationToken cancellationToken = default);

    // Kiểm tra student đã bookmark event này chưa
    Task<bool> IsBookmarkedAsync(Guid studentId, Guid eventId, CancellationToken cancellationToken = default);

    // Toggle: thêm nếu chưa có, xóa nếu đã có. Trả về true nếu sau đó là bookmarked.
    Task<bool> ToggleBookmarkAsync(Guid studentId, Guid eventId, CancellationToken cancellationToken = default);
}
