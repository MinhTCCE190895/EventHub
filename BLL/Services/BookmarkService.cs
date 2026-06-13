using BusinessObjects.DTOs;
using DAL.Data;
using DAL.Entities;
using DAL.Repositories;
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

    public async Task<bool> ToggleBookmarkAsync(Guid studentId, Guid eventId, CancellationToken cancellationToken = default)
    {
        var existing = await _bookmarkRepo.SingleOrDefaultAsync(
            b => b.StudentId == studentId && b.EventId == eventId,
            cancellationToken);

        if (existing != null)
        {
            _bookmarkRepo.Remove(existing);
            // Save changes directly on the DbContext for atomic deletion
            await _context.SaveChangesAsync(cancellationToken);
            return false;
        }

        var newBookmark = new Bookmark
        {
            StudentId = studentId,
            EventId = eventId,
            SavedAt = DateTime.UtcNow
        };

        await _bookmarkRepo.AddAsync(newBookmark, cancellationToken);
        // Save changes directly on the DbContext for atomic addition
        await _context.SaveChangesAsync(cancellationToken);
        return true;
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
            .Where(b => b.StudentId == studentId)
            .OrderByDescending(b => b.SavedAt)
            .ToListAsync(cancellationToken);

        return bookmarks.Select(b => new EventCardDTO
        {
            Id = b.Event.Id,
            Title = b.Event.Title,
            BannerUrl = b.Event.BannerUrl,
            StartTime = b.Event.StartTime,
            EndTime = b.Event.EndTime,
            VenueName = b.Event.Venue.Name,
            TagNames = b.Event.EventTags.Select(et => et.Tag.Name).ToList()
        }).ToList();
    }

    public async Task<bool> IsBookmarkedAsync(Guid studentId, Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _bookmarkRepo.ExistsAsync(
            b => b.StudentId == studentId && b.EventId == eventId,
            cancellationToken);
    }
}
