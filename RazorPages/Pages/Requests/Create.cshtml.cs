using BLL.Interfaces;
using BusinessObjects.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace RazorPages.Pages.Requests;

[Authorize(Roles = "Student")]
public class CreateModel : PageModel
{
    private readonly IEventRequestService _requestService;

    public CreateModel(IEventRequestService requestService)
    {
        _requestService = requestService;
    }

    [BindProperty]
    public EventRequestCreateDTO Input { get; set; } = new();

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            return RedirectToPage("/Logout");
        }

        try
        {
            await _requestService.CreateRequestAsync(userId, Input);
            TempData["SuccessMessage"] = "Đề xuất ý tưởng sự kiện đã được gửi thành công!";
            return RedirectToPage("./Index");
        }
        catch (System.Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return Page();
        }
    }
}
