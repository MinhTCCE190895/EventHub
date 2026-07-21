using BLL.Interfaces;
using BLL.DTOs;
using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class BookmarkService : IBookmarkService
{
    private readonly IRepository<Bookmark> _bookmarkRepo;
    private readonly AppDbContext _context;

    public BookmarkService(IRepository<Bookmark> bookmarkRepo, AppDbContext context)
    {
        _bookmarkRepo = bookmarkRepo;
        _context = context;
    }

    public async Task<List<Guid>> GetBookmarkedEventIdsAsync(Guid studentId, CancellationToken ct = default)
    {
        // Retrieve all bookmarked EventIds so the Index page knows which toggle buttons to highlight
        return await _context.Bookmarks
            .Where(b => b.StudentId == studentId)
            .Select(b => b.EventId)
            .ToListAsync(ct);
    }

    public async Task<List<EventCardDTO>> GetBookmarkedEventsAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        // Query bookmarks with related Event data using JOINs to avoid N+1 queries
        var bookmarks = await _context.Bookmarks
            .AsNoTracking()
            .Include(b => b.Event)
                .ThenInclude(e => e.Venue)
            .Include(b => b.Event)
                .ThenInclude(e => e.EventTags)
                    .ThenInclude(et => et.Tag)
            .Include(b => b.Event)
                .ThenInclude(e => e.Bookings)
            .Where(b => b.StudentId == studentId)
            .OrderByDescending(b => b.SavedAt)
            .ToListAsync(cancellationToken);

        return bookmarks.Select(b => new EventCardDTO
        {
            Id          = b.Event.Id,
            Title       = b.Event.Title,
            BannerUrl   = b.Event.BannerUrl,
            StartTime   = b.Event.StartTime.AddHours(7), // UTC → UTC+7
            EndTime     = b.Event.EndTime.AddHours(7),
            VenueName   = b.Event.Venue?.Name ?? "",
            TagNames    = b.Event.EventTags.Select(et => et.Tag.Name).ToList(),
            BookedCount = b.Event.Bookings.Count,
            MaxCapacity = b.Event.Venue?.MaxCapacity ?? 0
        }).ToList();
    }

    public async Task<bool> IsBookmarkedAsync(Guid studentId, Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _bookmarkRepo.ExistsAsync(
            b => b.StudentId == studentId && b.EventId == eventId,
            cancellationToken);
    }

    public async Task<bool> ToggleBookmarkAsync(Guid studentId, Guid eventId, CancellationToken cancellationToken = default)
    {
        var existing = await _bookmarkRepo.SingleOrDefaultAsync(
            b => b.StudentId == studentId && b.EventId == eventId,
            cancellationToken);

        if (existing is not null)
        {
            // If already bookmarked, remove it (unBookmark)
            _bookmarkRepo.Remove(existing);
            await _context.SaveChangesAsync(cancellationToken);
            return false;
        }

        // If not bookmarked yet, add a new one
        await _bookmarkRepo.AddAsync(new Bookmark
        {
            StudentId = studentId,
            EventId   = eventId,
            SavedAt   = DateTime.UtcNow
        }, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
