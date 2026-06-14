using BLL.Services;
using BusinessObjects.DTOs;
using DAL.Entities;
using DAL.Repositories;
using DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPages.ViewModels;

namespace RazerPages.Pages;

public class IndexModel : PageModel
{
    private readonly ISearchService _searchService;
    private readonly IRepository<Category> _categoryRepo;
    private readonly IRepository<Tag> _tagRepo;
    private readonly IBookmarkService _bookmarkService;
    private readonly IFollowService _followService;
    private readonly IRepository<User> _userRepo;
    private readonly AppDbContext _context;
    private readonly ILogger<IndexModel> _logger;

    public static readonly Guid CurrentStudentId = new("77777777-7777-7777-7777-777777777777");

    public IndexModel(
        ISearchService searchService,
        IRepository<Category> categoryRepo,
        IRepository<Tag> tagRepo,
        IBookmarkService bookmarkService,
        IFollowService followService,
        IRepository<User> userRepo,
        AppDbContext context,
        ILogger<IndexModel> logger)
    {
        _searchService = searchService;
        _categoryRepo = categoryRepo;
        _tagRepo = tagRepo;
        _bookmarkService = bookmarkService;
        _followService = followService;
        _userRepo = userRepo;
        _context = context;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public EventSearchViewModel SearchVm { get; set; } = new();

    public List<Guid> BookmarkedEventIds { get; set; } = new();
    public List<Guid> FollowedOrganizerIds { get; set; } = new();
    public Dictionary<Guid, int> OrganizerFollowersCounts { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return Page();

        await EnsureStudentExistsAsync(cancellationToken);

        // Fetch all bookmarked event IDs for the current student to display active states on UI cards
        BookmarkedEventIds = await _context.Bookmarks
            .Where(b => b.StudentId == CurrentStudentId)
            .Select(b => b.EventId)
            .ToListAsync(cancellationToken);

        // Fetch follow states for the current student
        FollowedOrganizerIds = await _context.Follows
            .Where(f => f.FollowerId == CurrentStudentId)
            .Select(f => f.FolloweeId)
            .ToListAsync(cancellationToken);

        try
        {
            SearchVm.Categories = (await _categoryRepo.GetAllAsync(cancellationToken)).ToList();
            SearchVm.Tags = (await _tagRepo.GetAllAsync(cancellationToken)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không load được danh sách category/tag trên trang chủ");
        }

        var searchDto = new EventSearchDTO
        {
            Keyword = SearchVm.Keyword,
            CategoryId = SearchVm.CategoryId,
            TagIds = SearchVm.TagIds,
            TimeFilter = SearchVm.TimeFilter,
            StartDate = SearchVm.StartDate,
            EndDate = SearchVm.EndDate,
            PageNumber = SearchVm.PageNumber,
            SortBy = SearchVm.SortBy
        };

        var (items, totalCount) = await _searchService.SearchEventsAsync(searchDto, cancellationToken);

        SearchVm.Results = items;
        SearchVm.TotalCount = totalCount;

        // Fetch followers count for organizers of the displayed events using optimized .CountAsync() grouping
        var organizerIds = items.Select(i => i.OrganizerId).Distinct().ToList();
        OrganizerFollowersCounts = await _context.Follows
            .Where(f => organizerIds.Contains(f.FolloweeId))
            .GroupBy(f => f.FolloweeId)
            .Select(g => new { OrganizerId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.OrganizerId, x => x.Count, cancellationToken);

        // Clamp page number to valid range to prevent out of bounds
        SearchVm.PageNumber = Math.Clamp(SearchVm.PageNumber, 1, Math.Max(1, SearchVm.TotalPages));

        return Page();
    }

    public async Task<IActionResult> OnPostToggleBookmarkAsync(Guid eventId, CancellationToken cancellationToken)
    {
        await EnsureStudentExistsAsync(cancellationToken);
        try
        {
            var isBookmarked = await _bookmarkService.ToggleBookmarkAsync(CurrentStudentId, eventId, cancellationToken);
            return new JsonResult(new { success = true, isBookmarked });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling bookmark for event {EventId}", eventId);
            return new JsonResult(new { success = false, error = "Failed to toggle bookmark" });
        }
    }

    public async Task<IActionResult> OnPostToggleFollowAsync(Guid organizerId, CancellationToken cancellationToken)
    {
        await EnsureStudentExistsAsync(cancellationToken);
        try
        {
            var isFollowing = await _followService.ToggleFollowAsync(CurrentStudentId, organizerId, cancellationToken);
            var count = await _followService.GetFollowersCountAsync(organizerId, cancellationToken);
            return new JsonResult(new { success = true, isFollowing, count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling follow for organizer {OrganizerId}", organizerId);
            return new JsonResult(new { success = false, error = ex.Message });
        }
    }

    private async Task EnsureStudentExistsAsync(CancellationToken cancellationToken)
    {
        var exists = await _userRepo.ExistsAsync(u => u.Id == CurrentStudentId, cancellationToken);
        if (!exists)
        {
            // Create a mock student if not exists to ensure db constraints are satisfied
            var student = new User
            {
                Id = CurrentStudentId,
                FullName = "Demo Student (QuiNC)",
                Role = "Student",
                Email = "student.demo@unieventhub.com",
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
            await _userRepo.AddAsync(student, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
