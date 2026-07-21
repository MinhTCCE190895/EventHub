using BLL.Services;
using BLL.Interfaces;
using BLL.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPages.Pages.Events;

[Authorize(Roles = "Admin,Organizer")]
public class DeleteModel : PageModel
{
    private readonly IEventService _eventService;

    public DeleteModel(IEventService eventService)
    {
        _eventService = eventService;
    }

    public EventDTO EventItem { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        var ev = await _eventService.GetEventByIdAsync(id);
        if (ev == null) return NotFound();

        EventItem = ev;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(Guid id)
    {
        try
        {
            await _eventService.DeleteEventAsync(id);
            TempData["SuccessMessage"] = "Sự kiện đã được xóa thành công!";
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("./Delete", new { id });
        }

        return RedirectToPage("./Index");
    }
}
