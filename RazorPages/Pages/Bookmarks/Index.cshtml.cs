using BLL.Services;
using BusinessObjects.DTOs;
using DAL.Data;
using DAL.Entities;
using DAL.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPages.Pages.Bookmarks;

public class IndexModel : PageModel
{
    private readonly IBookmarkService _bookmarkService;
    private readonly IRepository<User> _userRepo;
    private readonly AppDbContext _context;
    private readonly ILogger<IndexModel> _logger;

    public static readonly Guid CurrentStudentId = new("77777777-7777-7777-7777-777777777777");

    public IndexModel(
        IBookmarkService bookmarkService,
        IRepository<User> userRepo,
        AppDbContext context,
        ILogger<IndexModel> logger)
    {
        _bookmarkService = bookmarkService;
        _userRepo = userRepo;
        _context = context;
        _logger = logger;
    }

    public List<EventCardDTO> BookmarkedEvents { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        await EnsureStudentExistsAsync(cancellationToken);
        BookmarkedEvents = await _bookmarkService.GetBookmarkedEventsAsync(CurrentStudentId, cancellationToken);
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
