using BLL.Services;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPages.Pages.Organizers;

public class DetailsModel : PageModel
{
    private readonly IOrganizerService _organizerService;

    public DetailsModel(IOrganizerService organizerService)
    {
        _organizerService = organizerService;
    }

    public OrganizerDTO Organizer { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(Guid? id, CancellationToken cancellationToken)
    {
        if (id == null)
        {
            return NotFound();
        }

        var organizer = await _organizerService.GetOrganizerByIdAsync(id.Value, cancellationToken);
        if (organizer == null)
        {
            return NotFound();
        }

        Organizer = organizer;
        return Page();
    }
}
