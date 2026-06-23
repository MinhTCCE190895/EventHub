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

        // Chỉ lấy sự kiện đã Publish để tránh sinh viên nhìn thấy các bản nháp đang chỉnh sửa của Organizer
        query = query.Where(e => e.Status == "Published");

        var keyword = searchDto.Keyword?.Trim();
        if (!string.IsNullOrEmpty(keyword))
        {
            // Tìm cả trong tiêu đề lẫn mô tả để tăng khả năng khớp kết quả cho sinh viên
            query = query.Where(e => e.Title.Contains(keyword) || e.Description.Contains(keyword));
        }

        if (searchDto.CategoryId.HasValue && searchDto.CategoryId > 0)
        {
            // Cần chặn > 0 vì khi chọn "Tất cả danh mục" phía FE sẽ truyền mặc định là 0
            query = query.Where(e => e.EventCategories.Any(ec => ec.CategoryId == searchDto.CategoryId));
        }

        if (searchDto.TagIds.Count > 0)
        {
            // Áp dụng bộ lọc Any (OR) để hiển thị nhanh mọi sự kiện chứa ít nhất một thẻ sinh viên tích chọn
            query = query.Where(e => e.EventTags.Any(et => searchDto.TagIds.Contains(et.TagId)));
        }

        // Dùng múi giờ UTC gốc để so sánh chính xác với dữ liệu lưu dưới DB
        var now = DateTime.UtcNow;
        query = searchDto.TimeFilter switch
        {
            "Upcoming" => query.Where(e => e.StartTime > now),
            "Ongoing" => query.Where(e => e.StartTime <= now && e.EndTime >= now),
            "Past" => query.Where(e => e.EndTime < now),
            // Trạng thái mặc định: Hiển thị toàn bộ sự kiện không lọc thời gian
            _ => query
        };

        if (searchDto.StartDate.HasValue)
        {
            // Cho phép sinh viên lọc thủ công sự kiện diễn ra bắt đầu từ ngày mong muốn
            query = query.Where(e => e.StartTime >= searchDto.StartDate.Value);
        }

        if (searchDto.EndDate.HasValue)
        {
            // Cộng thêm 1 ngày trừ đi 1 tick để lấy trọn vẹn đến 23:59:59 của ngày kết thúc lọc
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
            // Mặc định gom các sự kiện kết thúc đẩy xuống dưới cùng để sinh viên dễ theo dõi các sự kiện đang/sắp chạy
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
