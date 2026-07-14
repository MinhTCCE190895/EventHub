using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.Services;
using BLL.Interfaces;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RazorPages.Pages.Venues;

[Authorize(Roles = "Admin,Organizer")]
public class CreateModel : PageModel
{
    private readonly IVenueService _venueService;

    public CreateModel(IVenueService venueService)
    {
        _venueService = venueService;
    }

    [BindProperty]
    public VenueCreateDTO VenueCreateDTO { get; set; } = default!;

    [BindProperty]
    public string? SelectedCampus { get; set; }

    public List<SelectListItem> Campuses { get; } = new List<SelectListItem>
    {
        new SelectListItem { Value = "Hồ Chí Minh", Text = "Hồ Chí Minh" },
        new SelectListItem { Value = "Hà Nội", Text = "Hà Nội" },
        new SelectListItem { Value = "Cần Thơ", Text = "Cần Thơ" },
        new SelectListItem { Value = "Đà Nẵng", Text = "Đà Nẵng" },
        new SelectListItem { Value = "Quy Nhơn", Text = "Quy Nhơn" }
    };

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!string.IsNullOrEmpty(SelectedCampus))
        {
            var suffix = ", " + SelectedCampus;
            if (!VenueCreateDTO.Address.EndsWith(suffix))
            {
                VenueCreateDTO.Address += suffix;
            }
        }

        try
        {
            await _venueService.CreateVenueAsync(VenueCreateDTO);
            TempData["SuccessMessage"] = "Venue created successfully!";
            return RedirectToPage("./Index");
        }
        catch (System.Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return Page();
        }
    }
}
