using BLL.DTOs;
using BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace RazorPages.Pages.Events;

[Authorize(Roles = "Student")]
public class MyFeedbacksModel : PageModel
{
    private readonly IFeedbackAnalyticsService _feedbackService;

    public MyFeedbacksModel(IFeedbackAnalyticsService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    public IEnumerable<StudentFeedbackDto> Feedbacks { get; set; } = new List<StudentFeedbackDto>();

    public async Task<IActionResult> OnGetAsync()
    {
        var studentIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(studentIdString) || !Guid.TryParse(studentIdString, out var studentId))
        {
            return RedirectToPage("/Account/Login");
        }

        Feedbacks = await _feedbackService.GetFeedbacksByStudentIdAsync(studentId);
        return Page();
    }
}
