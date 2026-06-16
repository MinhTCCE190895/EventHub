using BLL.Services;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace RazorPages.Pages.Organizers;

public class CreateModel : PageModel
{
    private readonly IOrganizerService _organizerService;

    public CreateModel(IOrganizerService organizerService)
    {
        _organizerService = organizerService;
    }

    [BindProperty]
    public OrganizerCreateDTO Organizer { get; set; } = default!;

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _organizerService.CreateOrganizerAsync(Organizer, cancellationToken);
            TempData["SuccessMessage"] = "Organizer created successfully.";
            return RedirectToPage("./Index");
        }
        catch (DbUpdateException ex)
        {
            ModelState.AddModelError(string.Empty, "An error occurred while saving the organizer. They might already exist.");
            return Page();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An unexpected error occurred: {ex.Message}");
            return Page();
        }
    }
}
