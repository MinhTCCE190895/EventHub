using BLL.Services;
using DAL.Data;
using DAL.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace RazorPages.Pages.Feedbacks;

public class SubmitModel : PageModel
{
    private readonly IFeedbackService _feedbackService;
    private readonly AppDbContext _context;
    private readonly ILogger<SubmitModel> _logger;

    public SubmitModel(
        IFeedbackService feedbackService,
        AppDbContext context,
        ILogger<SubmitModel> _logger)
    {
        _feedbackService = feedbackService;
        _context = context;
        this._logger = _logger;
    }

    [BindProperty(SupportsGet = true)]
    public Guid EventId { get; set; }

    public Event Event { get; set; } = null!;

    [BindProperty]
    public string StudentEmail { get; set; } = string.Empty;

    [BindProperty]
    public string? GeneralComment { get; set; }

    [BindProperty]
    public int SpeakerScore { get; set; } = 5;

    [BindProperty]
    public int LogisticsScore { get; set; } = 5;

    [BindProperty]
    public int ContentScore { get; set; } = 5;

    public string? SuccessMessage { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken cancellationToken)
    {
        var dbEvent = await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == EventId, cancellationToken);

        if (dbEvent == null)
        {
            return NotFound("Không tìm thấy sự kiện.");
        }

        Event = dbEvent;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        var dbEvent = await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == EventId, cancellationToken);

        if (dbEvent == null)
        {
            return NotFound("Không tìm thấy sự kiện.");
        }

        Event = dbEvent;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var student = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == StudentEmail && u.Role == "Student", cancellationToken);

            if (student == null)
            {
                ErrorMessage = "Không tìm thấy sinh viên với email này trong hệ thống.";
                return Page();
            }

            var canFeedback = await _feedbackService.CanUserLeaveFeedbackAsync(student.Id, EventId, cancellationToken);
            if (!canFeedback)
            {
                ErrorMessage = "Bạn không có vé xác nhận (Confirmed) cho sự kiện này hoặc đã gửi đánh giá trước đó.";
                return Page();
            }

            var criteriaScores = new List<(string Criteria, int Score)>
            {
                ("Diễn giả", SpeakerScore),
                ("Hậu cần", LogisticsScore),
                ("Nội dung", ContentScore)
            };

            await _feedbackService.SubmitFeedbackAsync(student.Id, EventId, GeneralComment, criteriaScores, cancellationToken);

            SuccessMessage = "Gửi đánh giá thành công! Cảm ơn ý kiến đóng góp của bạn.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting feedback for EventId: {EventId}", EventId);
            ErrorMessage = ex.Message;
        }

        return Page();
    }
}
