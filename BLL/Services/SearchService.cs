using BusinessObjects.DTOs;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class SearchService : ISearchService
{
    private readonly IEventRepository _eventRepo;

    public SearchService(IEventRepository eventRepo)
    {
        _eventRepo = eventRepo;
    }

    public async Task<(List<EventCardDTO> Items, int TotalCount)> SearchEventsAsync(
        EventSearchDTO searchDto,
        CancellationToken cancellationToken = default)
    {
        var query = _eventRepo.BuildSearchQuery();

        // Only show published events
        query = query.Where(e => e.Status == "Published");

        var keyword = searchDto.Keyword?.Trim();
        if (!string.IsNullOrEmpty(keyword))
        {
            // EF Core maps Contains to LIKE in SQL, no need for manual ToLower
            query = query.Where(e => e.Title.Contains(keyword) || e.Description.Contains(keyword));
        }

        if (searchDto.CategoryId.HasValue && searchDto.CategoryId > 0)
        {
            query = query.Where(e => e.EventCategories.Any(ec => ec.CategoryId == searchDto.CategoryId));
        }

        if (searchDto.TagIds.Count > 0)
        {
            // Event is valid if it matches at least one filtered tag (OR logic)
            query = query.Where(e => e.EventTags.Any(et => searchDto.TagIds.Contains(et.TagId)));
        }

        // Get current time once for consistent comparisons
        var now = DateTime.UtcNow;
        query = searchDto.TimeFilter switch
        {
            "Upcoming" => query.Where(e => e.StartTime > now),
            "Ongoing" => query.Where(e => e.StartTime <= now && e.EndTime >= now),
            "Past" => query.Where(e => e.EndTime < now),
            // Default: Show all events (no filter) when "Tất cả" is selected
            _ => query
        };

        if (searchDto.StartDate.HasValue)
        {
            // Filter events starting from the chosen date
            query = query.Where(e => e.StartTime >= searchDto.StartDate.Value);
        }

        if (searchDto.EndDate.HasValue)
        {
            // Filter events starting before the end of the chosen date
            var endOfDay = searchDto.EndDate.Value.Date.AddDays(1).AddTicks(-1);
            query = query.Where(e => e.StartTime <= endOfDay);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = searchDto.SortBy switch
        {
            "DateDesc" => query.OrderByDescending(e => e.StartTime),
            "NameAsc" => query.OrderBy(e => e.Title),
            "NameDesc" => query.OrderByDescending(e => e.Title),
            "Popularity" => query.OrderByDescending(e => e.Bookings.Count),
            // Default: Prioritize ongoing events first, then upcoming events ordered by start time
            _ => query.OrderBy(e => e.StartTime > now).ThenBy(e => e.StartTime)
        };

        var skip = (searchDto.PageNumber - 1) * EventSearchDTO.PageSize;
        var events = await query
            .Skip(skip)
            .Take(EventSearchDTO.PageSize)
            .ToListAsync(cancellationToken);

        var items = events.Select(e => new EventCardDTO
        {
            Id = e.Id,
            Title = e.Title,
            BannerUrl = e.BannerUrl,
            StartTime = e.StartTime,
            EndTime = e.EndTime,
            VenueName = e.Venue.Name,
            TagNames = e.EventTags.Select(et => et.Tag.Name).ToList(),
            OrganizerName = e.Organizer.FullName,
            MaxCapacity = e.Venue.MaxCapacity,
            BookedCount = e.Bookings.Count(b => b.Status != "Cancelled")
        }).ToList();

        return (items, totalCount);
    }
}
