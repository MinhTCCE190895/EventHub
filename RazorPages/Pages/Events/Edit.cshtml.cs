using BLL.Services;
using BLL.Interfaces;
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
    private readonly ICategoryService _categoryService;
    private readonly ITagService _tagService;

    public EditModel(
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
    public EventUpdateDTO Input { get; set; } = default!;

    public List<SelectListItem> Venues { get; set; } = new();
    public List<SelectListItem> Organizers { get; set; } = new();
    public List<SelectListItem> Categories { get; set; } = new();
    public List<SelectListItem> Tags { get; set; } = new();

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
            VenueId     = ev.VenueId,
            OrganizerId = ev.OrganizerId,
            CategoryIds = ev.CategoryIds,
            TagIds      = ev.TagIds
        };

        await LoadDropdownsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input != null && Input.StartTime >= Input.EndTime)
        {
            ModelState.AddModelError("Input.EndTime", "Ngày kết thúc phải sau ngày bắt đầu.");
        }

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
        catch (ArgumentException ex)
        {
            ModelState.AddModelError("Input.EndTime", ex.Message);
            await LoadDropdownsAsync();
            return Page();
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("Input.VenueId", ex.Message);
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
