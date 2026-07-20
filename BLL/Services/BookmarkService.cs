using BLL.Interfaces;
using BLL.DTOs;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class BookmarkService(IRepository<Bookmark> bookmarkRepo) : IBookmarkService
{
    private readonly IRepository<Bookmark> _bookmarkRepo = bookmarkRepo;

    // Lấy danh sách ID các sự kiện đã được lưu của sinh viên
    public async Task<List<Guid>> GetBookmarkedEventIdsAsync(Guid studentId, CancellationToken ct = default)
    {
        return await _bookmarkRepo.Query()
            .Where(b => b.StudentId == studentId)
            .Select(b => b.EventId)
            .ToListAsync(ct);
    }

    // Lấy thông tin chi tiết danh sách sự kiện đã lưu (sử dụng Projection .Select để tối ưu truy vấn)
    public async Task<List<EventCardDTO>> GetBookmarkedEventsAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        return await _bookmarkRepo.Query()
            .Where(b => b.StudentId == studentId)
            .OrderByDescending(b => b.SavedAt)
            .Select(b => new EventCardDTO
            {
                Id          = b.Event.Id,
                Title       = b.Event.Title,
                BannerUrl   = b.Event.BannerUrl,
                StartTime   = b.Event.StartTime.AddHours(7), // Đổi sang múi giờ VN (UTC+7)
                EndTime     = b.Event.EndTime.AddHours(7),
                VenueName   = b.Event.Venue != null ? b.Event.Venue.Name : "",
                TagNames    = b.Event.EventTags.Select(et => et.Tag.Name).ToList(),
                BookedCount = b.Event.Bookings.Count,
                MaxCapacity = b.Event.Venue != null ? b.Event.Venue.MaxCapacity : 0
            })
            .ToListAsync(cancellationToken);
    }

    // Kiểm tra xem một sự kiện cụ thể đã được lưu chưa
    public async Task<bool> IsBookmarkedAsync(Guid studentId, Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _bookmarkRepo.ExistsAsync(
            b => b.StudentId == studentId && b.EventId == eventId,
            cancellationToken);
    }

    // Toggle lưu/bỏ lưu sự kiện (nếu đã lưu thì xóa, chưa lưu thì thêm mới)
    public async Task<bool> ToggleBookmarkAsync(Guid studentId, Guid eventId, CancellationToken cancellationToken = default)
    {
        var existing = await _bookmarkRepo.SingleOrDefaultAsync(
            b => b.StudentId == studentId && b.EventId == eventId,
            cancellationToken);

        if (existing is not null)
        {
            // Đã lưu -> Tiến hành xóa lưu (un-bookmark)
            _bookmarkRepo.Remove(existing);
            return false;
        }

        // Chưa lưu -> Tạo mới bookmark
        await _bookmarkRepo.AddAsync(new Bookmark
        {
            StudentId = studentId,
            EventId   = eventId,
            SavedAt   = DateTime.UtcNow
        }, cancellationToken);

        return true;
    }
}
