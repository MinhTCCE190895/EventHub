using BLL.Interfaces;
using BLL.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace RazerPages.Pages;

public class IndexModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly ICategoryService _categoryService;
    private readonly ITagService _tagService;
    private readonly IBookmarkService _bookmarkService;
    private readonly ILogger<IndexModel> _logger;

    // Lấy ID của sinh viên hiện tại đăng nhập để hiển thị nút bookmark
    public Guid? CurrentStudentId => User.Identity?.IsAuthenticated == true
        ? Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!)
        : null;

    public IndexModel(
        IEventService eventService,
        ICategoryService categoryService,
        ITagService tagService,
        IBookmarkService bookmarkService,
        ILogger<IndexModel> logger)
    {
        _eventService    = eventService;
        _categoryService = categoryService;
        _tagService      = tagService;
        _bookmarkService = bookmarkService;
        _logger          = logger;
    }

    [BindProperty(SupportsGet = true)]
    public string? Keyword { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? CategoryId { get; set; }

    [BindProperty(SupportsGet = true)]
    public List<int> TagIds { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? TimeFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? EndDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public string? SortBy { get; set; } = "DateAsc";

    [BindProperty(SupportsGet = true)]
    public string ViewType { get; set; } = "Grid";

    // Danh sách kết quả trả về sau khi tìm kiếm
    public List<EventCardDTO> Results { get; set; } = new();

    public int TotalCount { get; set; }

    public int TotalPages => (int)Math.Ceiling((double)TotalCount / EventSearchDTO.PageSize);

    // Dùng để render danh mục và thẻ trên form lọc
    public List<CategoryDTO> Categories { get; set; } = new();
    public List<TagDTO> Tags { get; set; } = new();

    public List<Guid> BookmarkedEventIds { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        // Validate thủ công khoảng thời gian bắt đầu và kết thúc
        if (StartDate.HasValue && EndDate.HasValue && EndDate.Value < StartDate.Value)
        {
            ModelState.AddModelError(nameof(EndDate), "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
        }

        // Load danh mục và thẻ trước để hiển thị ra form bất kể có lỗi hay không
        try
        {
            Categories = (await _categoryService.GetAllCategoriesAsync(cancellationToken)).ToList();
            Tags       = (await _tagService.GetAllTagsAsync(cancellationToken)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không thể tải danh sách danh mục hoặc thẻ trên trang chủ");
        }

        if (!ModelState.IsValid)
            return Page();

        // Lấy danh sách bookmark của sinh viên nếu đã đăng nhập
        if (CurrentStudentId.HasValue && User.IsInRole("Student"))
        {
            BookmarkedEventIds = await _bookmarkService.GetBookmarkedEventIdsAsync(CurrentStudentId.Value, cancellationToken);
        }

        var searchDto = new EventSearchDTO
        {
            Keyword    = Keyword,
            CategoryId = CategoryId,
            TagIds     = TagIds,
            TimeFilter = TimeFilter,
            StartDate  = StartDate,
            EndDate    = EndDate,
            PageNumber = PageNumber,
            SortBy     = SortBy
        };

        var (items, totalCount) = await _eventService.SearchEventsAsync(searchDto, cancellationToken);

        Results    = items;
        TotalCount = totalCount;

        // Giới hạn số trang nằm trong khoảng hợp lệ
        PageNumber = Math.Clamp(PageNumber, 1, Math.Max(1, TotalPages));

        return Page();
    }

    // Toggle bookmark qua AJAX POST
    public async Task<IActionResult> OnPostToggleBookmarkAsync(Guid eventId, CancellationToken cancellationToken)
    {
        if (!CurrentStudentId.HasValue || !User.IsInRole("Student"))
            return Unauthorized();

        var isBookmarked = await _bookmarkService.ToggleBookmarkAsync(CurrentStudentId.Value, eventId, cancellationToken);
        return new JsonResult(new { bookmarked = isBookmarked });
    }
}
