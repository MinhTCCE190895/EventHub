using BLL.Services;
using BLL.Interfaces;
using BLL.DTOs;
using DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPages.Pages.Events
{
    public class DetailModel : PageModel
    {
        private readonly IEventService _eventService;
        private readonly IFeedbackAnalyticsService _feedbackService;
        private readonly IWeatherService _weatherService;
        private readonly ICommentService _commentService;
        private readonly IBookingService _bookingService;

        public DetailModel(
            IEventService eventService, 
            IFeedbackAnalyticsService feedbackService, 
            IWeatherService weatherService,
            ICommentService commentService,
            IBookingService bookingService)
        {
            _eventService = eventService;
            _feedbackService = feedbackService;
            _weatherService = weatherService;
            _commentService = commentService;
            _bookingService = bookingService;
        }

        public Event EventItem { get; set; } = default!;
        public bool ShowFeedbackButton { get; set; } = false;
        public WeatherDTO? EventWeather { get; set; }
        public bool IsForecastAvailable { get; set; }
        public IEnumerable<CommentDTO> Comments { get; set; } = new List<CommentDTO>();
        public BookingDTO? UserBooking { get; set; }
        public bool HasBooked => UserBooking != null;

        [BindProperty]
        public string CommentText { get; set; } = string.Empty;

        public int MaxCapacity => EventItem?.Venue?.MaxCapacity ?? 0;
        public int BookedCount => EventItem?.RegisteredCount ?? 0;
        public int RemainingSeats => MaxCapacity - BookedCount;
        public bool IsSoldOut => RemainingSeats <= 0;
        public bool IsPastEvent => EventItem != null && EventItem.EndTime.AddHours(7) < DateTime.UtcNow.AddHours(7);

        public double FillRate => MaxCapacity > 0 ? ((double)BookedCount / MaxCapacity) * 100 : 0;
        public string ProgressColor => FillRate >= 100 ? "bg-danger" : (FillRate >= 90 ? "bg-warning" : "bg-primary"); // clean-arch-ignore


        public string StatusText => EventItem == null ? string.Empty :
            EventItem.StartTime.AddHours(7) > DateTime.UtcNow.AddHours(7) ? "Sắp diễn ra" :
            IsPastEvent ? "Đã kết thúc" : "Đang diễn ra";

        public string StatusClass => EventItem == null ? string.Empty :
            EventItem.StartTime.AddHours(7) > DateTime.UtcNow.AddHours(7) ? "status-upcoming" :
            IsPastEvent ? "status-past" : "status-ongoing";

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var eventItem = await _eventService.GetEventEntityByIdAsync(id);

            if (eventItem == null)
            {
                return NotFound();
            }

            EventItem = eventItem;
            EventWeather = await _weatherService.GetWeatherForecastAsync(EventItem.Venue?.Address, EventItem.StartTime.AddHours(7));
            IsForecastAvailable = EventWeather != null;
            ShowFeedbackButton = await _feedbackService.CanSubmitFeedbackAsync(id, User);
            Comments = await _commentService.GetCommentsForEventAsync(id);

            if (User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdClaim, out var userId))
                {
                    UserBooking = await _bookingService.GetUserBookingForEventAsync(id, userId);
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCommentAsync(Guid id)
        {
            if (!User.Identity?.IsAuthenticated == true)
            {
                TempData["ErrorMessage"] = "Bạn cần đăng nhập để thực hiện bình luận.";
                return RedirectToPage(new { id });
            }

            if (string.IsNullOrWhiteSpace(CommentText))
            {
                TempData["ErrorMessage"] = "Nội dung bình luận không được để trống.";
                return RedirectToPage(new { id });
            }

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                TempData["ErrorMessage"] = "Không xác định được danh tính người dùng.";
                return RedirectToPage(new { id });
            }

            try
            {
                await _commentService.AddCommentAsync(id, userId, CommentText.Trim());
                TempData["SuccessMessage"] = "Bình luận của bạn đã được đăng thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostHideCommentAsync(Guid id, Guid commentId)
        {
            if (!User.IsInRole("Admin"))
            {
                TempData["ErrorMessage"] = "Chỉ Quản trị viên (Admin) mới có quyền ẩn bình luận.";
                return RedirectToPage(new { id });
            }

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var adminUserId))
            {
                TempData["ErrorMessage"] = "Không xác định được danh tính Admin.";
                return RedirectToPage(new { id });
            }

            try
            {
                await _commentService.HideCommentAsync(commentId, adminUserId);
                TempData["SuccessMessage"] = "Đã ẩn bình luận thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToPage(new { id });
        }
    }
}
