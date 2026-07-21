using BLL.Interfaces;
using BLL.DTOs;
using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class BookmarkService(IRepository<Bookmark> bookmarkRepo, AppDbContext context) : IBookmarkService
{
    private readonly IRepository<Bookmark> _bookmarkRepo = bookmarkRepo;
    private readonly AppDbContext _context = context;

    // Get the list of bookmarked event IDs for a student
    public async Task<List<Guid>> GetBookmarkedEventIdsAsync(Guid studentId, CancellationToken ct = default)
    {
        return await _bookmarkRepo.Query()
            .Where(b => b.StudentId == studentId)
            .Select(b => b.EventId)
            .ToListAsync(ct);
    }

    // Fetch detailed info of bookmarked events (using Select projection to optimize query)
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
                StartTime   = b.Event.StartTime.AddHours(7), // Convert UTC to Vietnam timezone (UTC+7)
                EndTime     = b.Event.EndTime.AddHours(7),
                VenueName   = b.Event.Venue != null ? b.Event.Venue.Name : "",
                TagNames    = b.Event.EventTags.Select(et => et.Tag.Name).ToList(),
                BookedCount = b.Event.Bookings.Count,
                MaxCapacity = b.Event.Venue != null ? b.Event.Venue.MaxCapacity : 0
            })
            .ToListAsync(cancellationToken);
    }

    // Check if a specific event has been bookmarked
    public async Task<bool> IsBookmarkedAsync(Guid studentId, Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _bookmarkRepo.ExistsAsync(
            b => b.StudentId == studentId && b.EventId == eventId,
            cancellationToken);
    }

    // Toggle bookmark status (remove if exists, add if new)
    public async Task<bool> ToggleBookmarkAsync(Guid studentId, Guid eventId, CancellationToken cancellationToken = default)
    {
        var existing = await _bookmarkRepo.SingleOrDefaultAsync(
            b => b.StudentId == studentId && b.EventId == eventId,
            cancellationToken);

        if (existing is not null)
        {
            // Already bookmarked -> Remove it
            _bookmarkRepo.Remove(existing);
            await _context.SaveChangesAsync(cancellationToken); // Save changes to database
            return false;
        }

        // Not bookmarked yet -> Create new bookmark
        await _bookmarkRepo.AddAsync(new Bookmark
        {
            StudentId = studentId,
            EventId   = eventId,
            SavedAt   = DateTime.UtcNow
        }, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken); // Save changes to database

        return true;
    }
}
