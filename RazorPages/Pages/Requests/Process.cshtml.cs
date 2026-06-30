using BLL.Interfaces;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPages.Pages.Requests;

[Authorize(Roles = "Admin,Organizer")]
public class ProcessModel : PageModel
{
    private readonly IEventRequestService _requestService;

    public ProcessModel(IEventRequestService requestService)
    {
        _requestService = requestService;
    }

    [BindProperty]
    public EventRequestProcessDTO Input { get; set; } = new();

    public EventRequestDTO? RequestDetail { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        RequestDetail = await _requestService.GetRequestByIdAsync(id);
        if (RequestDetail == null)
        {
            return NotFound();
        }

        Input = new EventRequestProcessDTO
        {
            Id = RequestDetail.Id,
            Status = RequestDetail.Status,
            ResponseMessage = RequestDetail.ResponseMessage
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            RequestDetail = await _requestService.GetRequestByIdAsync(Input.Id);
            return Page();
        }

        try
        {
            await _requestService.ProcessRequestAsync(Input);
            TempData["SuccessMessage"] = "Ý tưởng sự kiện đã được xử lý thành công!";
            return RedirectToPage("./Index");
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
