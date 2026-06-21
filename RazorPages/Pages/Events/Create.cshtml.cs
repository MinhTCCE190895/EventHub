using BLL.Services;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RazorPages.Pages.Events;

[Authorize(Roles = "Admin,Organizer")]
public class CreateModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IVenueService _venueService;

    public CreateModel(IEventService eventService, IVenueService venueService)
    {
        _eventService = eventService;
        _venueService = venueService;
    }

    [BindProperty]
    public EventCreateDTO Input { get; set; } = default!;

    public List<SelectListItem> Venues { get; set; } = new();
    public List<SelectListItem> Organizers { get; set; } = new();
    public List<SelectListItem> Statuses { get; } = new()
    {
        new SelectListItem { Value = "Upcoming",  Text = "Sắp diễn ra" },
        new SelectListItem { Value = "Ongoing",   Text = "Đang diễn ra" },
        new SelectListItem { Value = "Past",      Text = "Đã kết thúc" },
        new SelectListItem { Value = "Cancelled", Text = "Đã hủy" }
    };

    public async Task<IActionResult> OnGetAsync()
    {
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

        await _eventService.CreateEventAsync(Input);
        TempData["SuccessMessage"] = "Sự kiện đã được tạo thành công!";
        return RedirectToPage("./Index");
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
