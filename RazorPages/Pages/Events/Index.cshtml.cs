using BLL.Services;
using BLL.Interfaces;
using BLL.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPages.Pages.Events;

[Authorize(Roles = "Admin,Organizer")]
public class IndexModel : PageModel
{
    private readonly IEventService _eventService;

    public IndexModel(IEventService eventService)
    {
        _eventService = eventService;
    }

    public IEnumerable<EventDTO> Events { get; set; } = new List<EventDTO>();

    public async Task OnGetAsync()
    {
        Events = await _eventService.GetAllEventsAsync();
    }

    public async Task<IActionResult> OnPostChangeStatusAsync(Guid id, string status)
    {
        if (!User.IsInRole("Admin"))
        {
            return Forbid();
        }

        try
        {
            await _eventService.ChangeEventStatusAsync(id, status);
            TempData["SuccessMessage"] = "Cập nhật trạng thái sự kiện thành công!";
        }
        catch (System.Collections.Generic.KeyNotFoundException)
        {
            return NotFound();
        }

        return RedirectToPage();
    }
}