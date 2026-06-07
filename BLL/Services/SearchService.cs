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

        // Chỉ lấy event đã publish — draft và completed không hiển thị cho sinh viên
        query = query.Where(e => e.Status == "Published");

        var keyword = searchDto.Keyword?.Trim();
        if (!string.IsNullOrEmpty(keyword))
        {
            // EF Core tự convert Contains sang SQL LIKE — không cần ToLower thủ công
            query = query.Where(e => e.Title.Contains(keyword) || e.Description.Contains(keyword));
        }

        if (searchDto.CategoryId.HasValue && searchDto.CategoryId > 0)
        {
            query = query.Where(e => e.EventCategories.Any(ec => ec.CategoryId == searchDto.CategoryId));
        }

        if (searchDto.TagIds.Count > 0)
        {
            // OR logic: event có ít nhất 1 tag trong danh sách được chọn là đủ điều kiện
            query = query.Where(e => e.EventTags.Any(et => searchDto.TagIds.Contains(et.TagId)));
        }

        // Lấy now một lần để tất cả so sánh thời gian trong cùng request dùng cùng giá trị
        var now = DateTime.UtcNow;
        query = searchDto.TimeFilter switch
        {
            "Upcoming" => query.Where(e => e.StartTime > now),
            "Ongoing"  => query.Where(e => e.StartTime <= now && e.EndTime >= now),
            "Past"     => query.Where(e => e.EndTime < now),
            _          => query
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var skip = (searchDto.PageNumber - 1) * EventSearchDTO.PageSize;
        var events = await query
            .OrderBy(e => e.StartTime)
            .Skip(skip)
            .Take(EventSearchDTO.PageSize)
            .ToListAsync(cancellationToken);

        var items = events.Select(e => new EventCardDTO
        {
            Id        = e.Id,
            Title     = e.Title,
            BannerUrl = e.BannerUrl,
            StartTime = e.StartTime,
            VenueName = e.Venue.Name,
            TagNames  = e.EventTags.Select(et => et.Tag.Name).ToList()
        }).ToList();

        return (items, totalCount);
    }
}
