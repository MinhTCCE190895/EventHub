using BLL.Services;
using BLL.Interfaces;
using BLL.DTOs;
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
    private readonly ICategoryService _categoryService;
    private readonly ITagService _tagService;

    public CreateModel(
        IEventService eventService, 
        IVenueService venueService,
        ICategoryService categoryService,
        ITagService tagService)
    {
        _eventService = eventService;
        _venueService = venueService;
        _categoryService = categoryService;
        _tagService = tagService;
    }

    [BindProperty]
    public EventCreateDTO Input { get; set; } = default!;

    public List<SelectListItem> Venues { get; set; } = new();
    public List<SelectListItem> Organizers { get; set; } = new();
    public List<SelectListItem> Categories { get; set; } = new();
    public List<SelectListItem> Tags { get; set; } = new();
    public List<SelectListItem> Statuses { get; } = new()
    {
        new SelectListItem { Value = "Draft",     Text = "Bản nháp" },
        new SelectListItem { Value = "Published", Text = "Phát hành" },
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

        try
        {
            await _eventService.CreateEventAsync(Input);
            TempData["SuccessMessage"] = "Sự kiện đã được tạo thành công!";
            return RedirectToPage("./Index");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await LoadDropdownsAsync();
            return Page();
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

        var categories = await _categoryService.GetAllCategoriesAsync();
        Categories = categories.Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name
        }).ToList();

        var tags = await _tagService.GetAllTagsAsync();
        Tags = tags.Select(t => new SelectListItem
        {
            Value = t.Id.ToString(),
            Text = t.Name
        }).ToList();
    }
}
