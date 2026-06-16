using BLL.Services;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace RazorPages.Pages.Organizers;

public class DeleteModel : PageModel
{
    private readonly IOrganizerService _organizerService;

    public DeleteModel(IOrganizerService organizerService)
    {
        _organizerService = organizerService;
    }

    [BindProperty]
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

    public async Task<IActionResult> OnPostAsync(Guid? id, CancellationToken cancellationToken)
    {
        if (id == null)
        {
            return NotFound();
        }

        try
        {
            await _organizerService.DeleteOrganizerAsync(id.Value, cancellationToken);
            TempData["SuccessMessage"] = "Organizer deleted successfully.";
            return RedirectToPage("./Index");
        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError(string.Empty, "Cannot delete the organizer. They might have related data in the system.");
            
            // Reload the organizer info for the view
            var organizer = await _organizerService.GetOrganizerByIdAsync(id.Value, cancellationToken);
            if (organizer != null)
            {
                Organizer = organizer;
            }
            return Page();
        }
    }
}
