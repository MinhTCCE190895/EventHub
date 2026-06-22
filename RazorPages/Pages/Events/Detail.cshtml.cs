using BLL.Services;
using DAL.Data;
using DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace RazorPages.Pages.Events
{
    public class DetailModel : PageModel
    {
        private readonly AppDbContext _context;
        private readonly IFeedbackAnalyticsService _feedbackService;

        public DetailModel(AppDbContext context, IFeedbackAnalyticsService feedbackService)
        {
            _context = context;
            _feedbackService = feedbackService;
        }

        public Event EventItem { get; set; } = default!;
        public bool ShowFeedbackButton { get; set; } = false;

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var eventItem = await _context.Events
                .Include(e => e.Venue)
                .Include(e => e.Organizer)
                .Include(e => e.EventTags).ThenInclude(et => et.Tag)
                .Include(e => e.EventCategories).ThenInclude(ec => ec.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (eventItem == null)
            {
                return NotFound();
            }

            EventItem = eventItem;

            // Kiểm tra xem user hiện tại là Student và chưa feedback
            var studentIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(studentIdString) && Guid.TryParse(studentIdString, out var studentId))
            {
                ShowFeedbackButton = await _feedbackService.CanSubmitFeedbackAsync(id, studentId);
            }

            return Page();
        }
    }
}
