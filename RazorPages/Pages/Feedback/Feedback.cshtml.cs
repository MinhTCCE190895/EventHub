using BLL.DTOs;
using BLL.Services;
using BLL.Interfaces;
using DAL.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace RazorPages.Pages.Feedback;

[Authorize(Roles = "Student")]
public class FeedbackModel : PageModel
{
    private readonly IFeedbackAnalyticsService _feedbackService;
    private readonly AppDbContext _context;

    public FeedbackModel(IFeedbackAnalyticsService feedbackService, AppDbContext context)
    {
        _feedbackService = feedbackService;
        _context = context;
    }

    [BindProperty]
    public FeedbackInput Input { get; set; } = new();

    public DAL.Entities.Event EventItem { get; set; } = null!;
    public Guid BookingId { get; set; }

    public class FeedbackInput
    {
        public string? GeneralComment { get; set; }
        public int SpeakerScore { get; set; } = 5;
        public int LogisticsScore { get; set; } = 5;
        public int ContentScore { get; set; } = 5;
        public int OrganizationScore { get; set; } = 5;
    }

    public async Task<IActionResult> OnGetAsync(Guid eventId)
    {
        var studentIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(studentIdString) || !Guid.TryParse(studentIdString, out var studentId))
        {
            return RedirectToPage("/Account/Login");
        }

        // L?y thông tin Event
        var ev = await _context.Events
            .Include(e => e.Venue)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (ev == null)
        {
            return NotFound();
        }

        EventItem = ev;

        // Ki?m tra xem sinh viên dã d?t vé Confirmed chua và dã g?i Feedback chua
        var booking = await _context.Bookings
            .Include(b => b.Feedback)
            .FirstOrDefaultAsync(b => b.EventId == eventId && b.StudentId == studentId && b.Status == "Confirmed");

        if (booking == null)
        {
            TempData["ErrorMessage"] = "B?n chua dang ký ho?c chua thanh toán vé cho s? ki?n này.";
            return RedirectToPage("/Events/Detail", new { id = eventId });
        }

        if (booking.Feedback != null)
        {
            TempData["ErrorMessage"] = "B?n dã th?c hi?n dánh giá cho s? ki?n này r?i.";
            return RedirectToPage("/Events/Detail", new { id = eventId });
        }

        BookingId = booking.Id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid eventId, Guid bookingId)
    {
        var studentIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(studentIdString) || !Guid.TryParse(studentIdString, out var studentId))
        {
            return RedirectToPage("/Account/Login");
        }

        var ev = await _context.Events.FindAsync(eventId);
        if (ev == null) return NotFound();
        EventItem = ev;
        BookingId = bookingId;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Ki?m tra l?i tính h?p l?
        var canSubmit = await _feedbackService.CanSubmitFeedbackAsync(eventId, studentId);
        if (!canSubmit)
        {
            TempData["ErrorMessage"] = "B?n không có quy?n dánh giá ho?c dã dánh giá s? ki?n này.";
            return RedirectToPage("/Events/Detail", new { id = eventId });
        }

        var submission = new FeedbackSubmissionDto
        {
            BookingId = bookingId,
            GeneralComment = Input.GeneralComment,
            Details = new List<FeedbackDetailDto>
            {
                new() { Criteria = "Di?n gi?", Score = Input.SpeakerScore },
                new() { Criteria = "H?u c?n", Score = Input.LogisticsScore },
                new() { Criteria = "N?i dung", Score = Input.ContentScore },
                new() { Criteria = "T? ch?c", Score = Input.OrganizationScore }
            }
        };

        try
        {
            await _feedbackService.SubmitFeedbackAsync(submission);
            TempData["SuccessMessage"] = "C?m on b?n dã g?i dánh giá ph?n h?i!";
            return RedirectToPage("/Events/Detail", new { id = eventId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Có l?i x?y ra: " + ex.Message);
            return Page();
        }
    }
}
