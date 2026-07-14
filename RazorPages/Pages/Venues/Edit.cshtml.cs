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
public class EditModel : PageModel
{
    private readonly IVenueService _venueService;

    public EditModel(IVenueService venueService)
    {
        _venueService = venueService;
    }

    [BindProperty]
    public VenueUpdateDTO VenueUpdateDTO { get; set; } = default!;

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

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var venue = await _venueService.GetVenueByIdAsync(id);
        if (venue == null)
        {
            return NotFound();
        }

        // Theo yêu cầu: Không trích xuất (Chỉ hiển thị chuỗi gốc)
        VenueUpdateDTO = new VenueUpdateDTO
        {
            Id = venue.Id,
            Name = venue.Name,
            MaxCapacity = venue.MaxCapacity,
            Address = venue.Address,
            Description = venue.Description
        };

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
            var address = VenueUpdateDTO.Address?.Trim() ?? "";
            bool removed;
            do
            {
                removed = false;
                foreach (var campus in Campuses)
                {
                    var oldSuffix = ", " + campus.Value;
                    if (address.EndsWith(oldSuffix))
                    {
                        address = address.Substring(0, address.Length - oldSuffix.Length).Trim();
                        removed = true;
                    }
                }
            } while (removed);

            VenueUpdateDTO.Address = address + ", " + SelectedCampus;
        }

        try
        {
            await _venueService.UpdateVenueAsync(VenueUpdateDTO);
            TempData["SuccessMessage"] = "Venue updated successfully!";
        }
        catch (System.Collections.Generic.KeyNotFoundException)
        {
            return NotFound();
        }
        catch (System.Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return Page();
        }

        return RedirectToPage("./Index");
    }
}
