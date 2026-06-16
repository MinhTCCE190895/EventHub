using BLL.Services;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace RazorPages.Pages.Organizers;

public class EditModel : PageModel
{
    private readonly IOrganizerService _organizerService;

    public EditModel(IOrganizerService organizerService)
    {
        _organizerService = organizerService;
    }

    [BindProperty]
    public OrganizerUpdateDTO Organizer { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(Guid? id, CancellationToken cancellationToken)
    {
        if (id == null)
        {
            return NotFound();
        }

        var organizerDto = await _organizerService.GetOrganizerByIdAsync(id.Value, cancellationToken);
        if (organizerDto == null)
        {
            return NotFound();
        }

        Organizer = new OrganizerUpdateDTO
        {
            Id = organizerDto.Id,
            FullName = organizerDto.FullName,
            Email = organizerDto.Email,
            StudentCode = organizerDto.StudentCode,
            IsActive = organizerDto.IsActive
        };

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
            await _organizerService.UpdateOrganizerAsync(Organizer, cancellationToken);
            TempData["SuccessMessage"] = "Organizer updated successfully.";
            return RedirectToPage("./Index");
        }
        catch (DbUpdateConcurrencyException)
        {
            var exists = await _organizerService.GetOrganizerByIdAsync(Organizer.Id, cancellationToken) != null;
            if (!exists)
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"An error occurred while updating: {ex.Message}");
            return Page();
        }
    }
}
