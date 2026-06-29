using System.Threading.Tasks;
using BLL.Services;
using BLL.Interfaces;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPages.Pages.Venues;

[Authorize(Roles = "Admin,Organizer")]
public class DeleteModel : PageModel
{
    private readonly IVenueService _venueService;

    public DeleteModel(IVenueService venueService)
    {
        _venueService = venueService;
    }

    [BindProperty]
    public VenueDTO VenueDTO { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var venue = await _venueService.GetVenueByIdAsync(id);
        if (venue == null)
        {
            return NotFound();
        }

        VenueDTO = venue;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        try
        {
            await _venueService.DeleteVenueAsync(id);
            TempData["SuccessMessage"] = "Venue deleted successfully!";
        }
        catch (System.InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("./Delete", new { id });
        }
        catch (System.Collections.Generic.KeyNotFoundException)
        {
            return NotFound();
        }

        return RedirectToPage("./Index");
    }
}
