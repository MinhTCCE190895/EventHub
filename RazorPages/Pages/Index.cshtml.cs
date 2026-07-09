using BLL.Interfaces;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPages.ViewModels;
using System.Security.Claims;

namespace RazerPages.Pages;

public class IndexModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly ICategoryService _categoryService;
    private readonly ITagService _tagService;
    private readonly IBookmarkService _bookmarkService;
    private readonly ILogger<IndexModel> _logger;

    // Get actual user ID from claims instead of hardcoding
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
    public EventSearchViewModel SearchVm { get; set; } = new();

    public List<Guid> BookmarkedEventIds { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        // Retrieve bookmarks via BLL service — no direct DbContext at Presentation layer (ARCH-04 fix)
        if (CurrentStudentId.HasValue && User.IsInRole("Student"))
        {
            BookmarkedEventIds = await _bookmarkService.GetBookmarkedEventIdsAsync(CurrentStudentId.Value, cancellationToken);
        }

        try
        {
            SearchVm.Categories = (await _categoryService.GetAllCategoriesAsync(cancellationToken)).ToList();
            SearchVm.Tags       = (await _tagService.GetAllTagsAsync(cancellationToken)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load category/tag lists on home page");
        }

        var searchDto = new EventSearchDTO
        {
            Keyword    = SearchVm.Keyword,
            CategoryId = SearchVm.CategoryId,
            TagIds     = SearchVm.TagIds,
            TimeFilter = SearchVm.TimeFilter,
            StartDate  = SearchVm.StartDate,
            EndDate    = SearchVm.EndDate,
            PageNumber = SearchVm.PageNumber,
            SortBy     = SearchVm.SortBy
        };

        var (items, totalCount) = await _eventService.SearchEventsAsync(searchDto, cancellationToken);

        SearchVm.Results    = items;
        SearchVm.TotalCount = totalCount;

        // Clamp page number to valid range to prevent out of bounds
        SearchVm.PageNumber = Math.Clamp(SearchVm.PageNumber, 1, Math.Max(1, SearchVm.TotalPages));

        return Page();
    }

    // Toggle bookmark via AJAX POST — returns JSON {bookmarked: true/false}
    public async Task<IActionResult> OnPostToggleBookmarkAsync(Guid eventId, CancellationToken cancellationToken)
    {
        if (!CurrentStudentId.HasValue || !User.IsInRole("Student"))
            return Unauthorized();

        var isBookmarked = await _bookmarkService.ToggleBookmarkAsync(CurrentStudentId.Value, eventId, cancellationToken);
        return new JsonResult(new { bookmarked = isBookmarked });
    }
}
