using BLL.Interfaces;
using BLL.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace RazorPages.Pages.Bookmarks;

[Authorize(Roles = "Student")]
public class IndexModel(IBookmarkService bookmarkService, ILogger<IndexModel> logger) : PageModel
{
    private readonly IBookmarkService _bookmarkService = bookmarkService;
    private readonly ILogger<IndexModel> _logger = logger;

    private Guid CurrentStudentId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public List<EventCardDTO> BookmarkedEvents { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        BookmarkedEvents = await _bookmarkService.GetBookmarkedEventsAsync(CurrentStudentId, cancellationToken);
        return Page();
    }

    // AJAX POST — toggle bookmark; if unbookmarked, the card will automatically hide on the UI
    public async Task<IActionResult> OnPostToggleBookmarkAsync(Guid eventId, CancellationToken cancellationToken)
    {
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
}
