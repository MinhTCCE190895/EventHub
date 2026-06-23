using BLL.DTOs;
using BLL.Services;
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

        // Lấy thông tin Event
        var ev = await _context.Events
            .Include(e => e.Venue)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (ev == null)
        {
            return NotFound();
        }

        EventItem = ev;

        // Kiểm tra xem sinh viên đã đặt vé Confirmed chưa và đã gửi Feedback chưa
        var booking = await _context.Bookings
            .Include(b => b.Feedback)
            .FirstOrDefaultAsync(b => b.EventId == eventId && b.StudentId == studentId && b.Status == "Confirmed");

        if (booking == null)
        {
            TempData["ErrorMessage"] = "Bạn chưa đăng ký hoặc chưa thanh toán vé cho sự kiện này.";
            return RedirectToPage("/Events/Detail", new { id = eventId });
        }

        if (booking.Feedback != null)
        {
            TempData["ErrorMessage"] = "Bạn đã thực hiện đánh giá cho sự kiện này rồi.";
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

        // Kiểm tra lại tính hợp lệ
        var canSubmit = await _feedbackService.CanSubmitFeedbackAsync(eventId, studentId);
        if (!canSubmit)
        {
            TempData["ErrorMessage"] = "Bạn không có quyền đánh giá hoặc đã đánh giá sự kiện này.";
            return RedirectToPage("/Events/Detail", new { id = eventId });
        }

        var submission = new FeedbackSubmissionDto
        {
            BookingId = bookingId,
            GeneralComment = Input.GeneralComment,
            Details = new List<FeedbackDetailDto>
            {
                new() { Criteria = "Diễn giả", Score = Input.SpeakerScore },
                new() { Criteria = "Hậu cần", Score = Input.LogisticsScore },
                new() { Criteria = "Nội dung", Score = Input.ContentScore },
                new() { Criteria = "Tổ chức", Score = Input.OrganizationScore }
            }
        };

        try
        {
            await _feedbackService.SubmitFeedbackAsync(submission);
            TempData["SuccessMessage"] = "Cảm ơn bạn đã gửi đánh giá phản hồi!";
            return RedirectToPage("/Events/Detail", new { id = eventId });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, "Có lỗi xảy ra: " + ex.Message);
            return Page();
        }
    }
}
