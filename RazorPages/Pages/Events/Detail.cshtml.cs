using BLL.Services;
using BLL.Interfaces;
using BusinessObjects.DTOs;
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

        public DetailModel(IEventService eventService, IFeedbackAnalyticsService feedbackService, IWeatherService weatherService)
        {
            _eventService = eventService;
            _feedbackService = feedbackService;
            _weatherService = weatherService;
        }

        public Event EventItem { get; set; } = default!;
        public bool ShowFeedbackButton { get; set; } = false;
        public WeatherDTO? EventWeather { get; set; }
        public bool IsForecastAvailable { get; set; }

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

            return Page();
        }
    }
}
