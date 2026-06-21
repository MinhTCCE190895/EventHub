using BLL.Services;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RazorPages.Pages.Events;

[Authorize(Roles = "Admin,Organizer")]
public class EditModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IVenueService _venueService;

    public EditModel(IEventService eventService, IVenueService venueService)
    {
        _eventService = eventService;
        _venueService = venueService;
    }

    [BindProperty]
    public EventUpdateDTO Input { get; set; } = default!;

    public List<SelectListItem> Venues { get; set; } = new();
    public List<SelectListItem> Organizers { get; set; } = new();
    public List<SelectListItem> Statuses { get; } = new()
    {
        new SelectListItem { Value = "Upcoming",  Text = "Sắp diễn ra" },
        new SelectListItem { Value = "Ongoing",   Text = "Đang diễn ra" },
        new SelectListItem { Value = "Past",      Text = "Đã kết thúc" },
        new SelectListItem { Value = "Cancelled", Text = "Đã hủy" }
    };

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var ev = await _eventService.GetEventByIdAsync(id);
        if (ev == null) return NotFound();

        Input = new EventUpdateDTO
        {
            Id          = ev.Id,
            Title       = ev.Title,
            Description = ev.Description,
            BannerUrl   = ev.BannerUrl,
            StartTime   = ev.StartTime,
            EndTime     = ev.EndTime,
            Status      = ev.Status,
            VenueId     = ev.VenueId,
            OrganizerId = ev.OrganizerId
        };

        await LoadDropdownsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return Page();
        }

        try
        {
            await _eventService.UpdateEventAsync(Input);
            TempData["SuccessMessage"] = "Sự kiện đã được cập nhật thành công!";
            return RedirectToPage("./Index");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private async Task LoadDropdownsAsync()
    {
        var venues = await _venueService.GetAllVenuesAsync();
        Venues = venues.Select(v => new SelectListItem
        {
            Value = v.Id.ToString(),
            Text = $"{v.Name} (sức chứa: {v.MaxCapacity})"
        }).ToList();

        var organizers = await _eventService.GetOrganizersAsync();
        Organizers = organizers.Select(u => new SelectListItem
        {
            Value = u.Id.ToString(),
            Text = u.FullName
        }).ToList();
    }
}
