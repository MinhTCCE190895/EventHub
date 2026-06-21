using BLL.Services;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
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
}
