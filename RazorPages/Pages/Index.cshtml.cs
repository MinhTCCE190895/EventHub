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

    // Retrieve logged-in student's ID from Claims safely
    public Guid? CurrentStudentId
    {
        get
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                {
                    return Guid.Parse(userIdClaim);
                }
            }
            return null;
        }
    }

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
    public string? ViewType { get; set; } = "Grid";

    // Danh sách kết quả trả về sau khi tìm kiếm
    public List<EventCardDTO> Results { get; set; } = new();

    public int TotalCount { get; set; }

    public int TotalPages
    {
        get
        {
            double pages = (double)TotalCount / EventSearchDTO.PageSize;
            return (int)Math.Ceiling(pages);
        }
    }

    // Lists to populate categories and tags dropdown/checkbox selections on the UI
    public List<CategoryDTO> Categories { get; set; } = new();
    public List<TagDTO> Tags { get; set; } = new();

    public List<Guid> BookmarkedEventIds { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(ViewType))
        {
            ViewType = "Grid";
        }

        // Verify if EndDate is chronologically before StartDate to append field-level error
        if (StartDate.HasValue && EndDate.HasValue && EndDate.Value < StartDate.Value)
        {
            ModelState.AddModelError(nameof(EndDate), "Ngày kết thúc phải lớn hơn hoặc bằng ngày bắt đầu.");
        }

        // Pre-load category and tag lists to populate dropdowns regardless of validation state
        try
        {
            Categories = (await _categoryService.GetAllCategoriesAsync(cancellationToken)).ToList();
            Tags       = (await _tagService.GetAllTagsAsync(cancellationToken)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load category or tag lists on home page");
        }

        // Nếu có lỗi validation (như nhập sai khoảng ngày) thì dừng và hiển thị trang luôn
        if (!ModelState.IsValid)
            return Page();

        // Retrieve bookmarked event IDs for the logged-in student to display correct bookmark icon states
        if (CurrentStudentId.HasValue && User.IsInRole("Student"))
        {
            try
            {
                BookmarkedEventIds = await _bookmarkService.GetBookmarkedEventIdsAsync(CurrentStudentId.Value, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load bookmarks for student: {StudentId}", CurrentStudentId);
            }
        }

        // Map filter inputs from PageModel properties to EventSearchDTO for BLL consumption
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

        // Call business logic service to execute search and fetch total count
        var (items, totalCount) = await _eventService.SearchEventsAsync(searchDto, cancellationToken);

        Results    = items;
        TotalCount = totalCount;

        // Clamp page number between 1 and TotalPages to prevent out-of-bounds navigation queries
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
