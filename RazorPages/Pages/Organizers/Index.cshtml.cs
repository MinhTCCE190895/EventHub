using BLL.Services;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPages.Pages.Organizers;

public class IndexModel : PageModel
{
    private readonly IOrganizerService _organizerService;

    public IndexModel(IOrganizerService organizerService)
    {
        _organizerService = organizerService;
    }

    public IEnumerable<OrganizerDTO> Organizers { get; set; } = new List<OrganizerDTO>();

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        Organizers = await _organizerService.GetAllOrganizersAsync(cancellationToken);
        return Page();
    }
}
