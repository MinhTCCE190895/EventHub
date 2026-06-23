using AutoMapper;
using BusinessObjects.DTOs;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class SearchService : ISearchService
{
    private readonly IEventRepository _eventRepo;
    private readonly IMapper _mapper;

    public SearchService(IEventRepository eventRepo, IMapper mapper)
    {
        _eventRepo = eventRepo;
        _mapper = mapper;
    }

    public async Task<(List<EventCardDTO> Items, int TotalCount)> SearchEventsAsync(
        EventSearchDTO searchDto,
        CancellationToken cancellationToken = default)
    {
        var query = _eventRepo.BuildSearchQuery();

        // Only retrieve Published events to prevent students from seeing drafts being edited by organizers
        query = query.Where(e => e.Status == "Published");

        var keyword = searchDto.Keyword?.Trim();
        if (!string.IsNullOrEmpty(keyword))
        {
            // Search in both title and description to increase search accuracy for students
            query = query.Where(e => e.Title.Contains(keyword) || e.Description.Contains(keyword));
        }

        if (searchDto.CategoryId.HasValue && searchDto.CategoryId > 0)
        {
            // Need to check > 0 because selecting "All Categories" from the front-end defaults to 0
            query = query.Where(e => e.EventCategories.Any(ec => ec.CategoryId == searchDto.CategoryId));
        }

        if (searchDto.TagIds.Count > 0)
        {
            // Apply Any (OR) filter to display any event containing at least one selected tag
            query = query.Where(e => e.EventTags.Any(et => searchDto.TagIds.Contains(et.TagId)));
        }

        // Use UTC timezone to compare accurately with database values
        var now = DateTime.UtcNow;
        query = searchDto.TimeFilter switch
        {
            "Upcoming" => query.Where(e => e.StartTime > now),
            "Ongoing" => query.Where(e => e.StartTime <= now && e.EndTime >= now),
            "Past" => query.Where(e => e.EndTime < now),
            // Default state: Display all events without filtering by time
            _ => query
        };

        if (searchDto.StartDate.HasValue)
        {
            // Allow students to manually filter events starting from a specific date
            query = query.Where(e => e.StartTime >= searchDto.StartDate.Value);
        }

        if (searchDto.EndDate.HasValue)
        {
            // Add 1 day minus 1 tick to include up to 23:59:59 of the end date
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
            // Default: Push past events to the bottom so students can easily track active/upcoming events
            _ => query.OrderBy(e => e.EndTime < now)
                      .ThenBy(e => e.StartTime > now)
                      .ThenBy(e => e.StartTime)
        };

        var skip = (searchDto.PageNumber - 1) * EventSearchDTO.PageSize;
        var events = await query
            .Skip(skip)
            .Take(EventSearchDTO.PageSize)
            .ToListAsync(cancellationToken);

        var items = _mapper.Map<List<EventCardDTO>>(events);

        return (items, totalCount);
    }
}
