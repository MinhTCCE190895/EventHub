using BLL.Interfaces;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace RazorPages.Pages.Requests;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IEventRequestService _requestService;

    public IndexModel(IEventRequestService requestService)
    {
        _requestService = requestService;
    }

    public List<EventRequestDTO> Requests { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return RedirectToPage("/Logout");
        }

        if (User.IsInRole("Student"))
        {
            var myRequests = await _requestService.GetRequestsByStudentIdAsync(userId);
            Requests = myRequests.ToList();
        }
        else
        {
            var allRequests = await _requestService.GetAllRequestsAsync();
            if (!string.IsNullOrEmpty(StatusFilter))
            {
                allRequests = allRequests.Where(r => r.Status.Equals(StatusFilter, StringComparison.OrdinalIgnoreCase));
            }
            Requests = allRequests.ToList();
        }

        return Page();
    }
}
