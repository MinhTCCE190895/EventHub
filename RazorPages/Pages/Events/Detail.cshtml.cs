using BLL.Services;
using BLL.Interfaces;
using BusinessObjects.DTOs;
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
        private readonly IWeatherService _weatherService;

        public DetailModel(AppDbContext context, IFeedbackAnalyticsService feedbackService, IWeatherService weatherService)
        {
            _context = context;
            _feedbackService = feedbackService;
            _weatherService = weatherService;
        }

        public Event EventItem { get; set; } = default!;
        public bool ShowFeedbackButton { get; set; } = false;
        public WeatherDTO? EventWeather { get; set; }
        public bool IsForecastAvailable { get; set; }

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

            // Add 7 hours because the DB stores in UTC, need to convert to Vietnam time for accurate date comparison
            var today = DateTime.UtcNow.AddHours(7).Date;
            var targetDate = EventItem.StartTime.AddHours(7).Date;
            var daysDifference = (targetDate - today).Days;

            // API wttr.in only supports weather forecasts within the next 3 days (today, tomorrow, the day after)
            if (daysDifference >= 0 && daysDifference <= 2)
            {
                IsForecastAvailable = true;
                if (EventItem.Venue != null && !string.IsNullOrWhiteSpace(EventItem.Venue.Address))
                {
                    // Fetch weather forecast for Venue address to recommend suitable clothing for students
                    EventWeather = await _weatherService.GetWeatherForecastAsync(EventItem.Venue.Address, EventItem.StartTime.AddHours(7));
                }
            }
            else
            {
                IsForecastAvailable = false;
            }

            // Check if the current user is a Student and has not submitted feedback yet
            var studentIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(studentIdString) && Guid.TryParse(studentIdString, out var studentId))
            {
                ShowFeedbackButton = await _feedbackService.CanSubmitFeedbackAsync(id, studentId);
            }

            return Page();
        }
    }
}
