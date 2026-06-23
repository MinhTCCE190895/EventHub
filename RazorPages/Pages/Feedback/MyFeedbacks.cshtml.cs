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
public class MyFeedbacksModel : PageModel
{
    private readonly AppDbContext _context;

    public MyFeedbacksModel(AppDbContext context)
    {
        _context = context;
    }

    public class PendingFeedbackDto
    {
        public Guid BookingId { get; set; }
        public Guid EventId { get; set; }
        public string EventTitle { get; set; } = null!;
        public DateTime StartTime { get; set; }
        public string VenueName { get; set; } = null!;
        public string BannerUrl { get; set; } = null!;
    }

    public List<PendingFeedbackDto> PendingEvents { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var studentIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(studentIdString) || !Guid.TryParse(studentIdString, out var studentId))
        {
            return RedirectToPage("/Account/Login");
        }

        // Lấy toàn bộ danh sách sự kiện đã đăng ký thành công nhưng chưa thực hiện đánh giá (Feedback)
        PendingEvents = await _context.Bookings
            .Include(b => b.Event)
                .ThenInclude(e => e.Venue)
            .Where(b => b.StudentId == studentId && b.Status == "Confirmed" && b.Feedback == null)
            .OrderByDescending(b => b.BookingTime)
            .Select(b => new PendingFeedbackDto
            {
                BookingId = b.Id,
                EventId = b.EventId,
                EventTitle = b.Event.Title,
                StartTime = b.Event.StartTime,
                VenueName = b.Event.Venue.Name,
                BannerUrl = b.Event.BannerUrl
            })
            .ToListAsync();

        return Page();
    }
}
