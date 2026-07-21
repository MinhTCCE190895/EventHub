using System.Collections.Generic;
using System.Threading.Tasks;
using BLL.Services;
using BLL.Interfaces;
using BLL.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPages.Pages.Venues;

[Authorize(Roles = "Admin,Organizer")]
public class IndexModel : PageModel
{
    private readonly IVenueService _venueService;

    public IndexModel(IVenueService venueService)
    {
        _venueService = venueService;
    }

    public IEnumerable<VenueDTO> Venues { get; set; } = new List<VenueDTO>();

    public async Task OnGetAsync()
    {
        Venues = await _venueService.GetAllVenuesAsync();
    }
}
