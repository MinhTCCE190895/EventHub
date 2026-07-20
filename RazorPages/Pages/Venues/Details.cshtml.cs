using System.Threading.Tasks;
using BLL.Services;
using BLL.Interfaces;
using BLL.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPages.Pages.Venues;

[Authorize(Roles = "Admin,Organizer")]
public class DetailsModel : PageModel
{
    private readonly IVenueService _venueService;

    public DetailsModel(IVenueService venueService)
    {
        _venueService = venueService;
    }

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
}
