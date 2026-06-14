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

namespace RazorPages.Pages.Feedbacks;

public class AnalyticsModel : PageModel
{
    private readonly IFeedbackAnalyticsService _analyticsService;
    private readonly AppDbContext _context;

    public AnalyticsModel(IFeedbackAnalyticsService analyticsService, AppDbContext context)
    {
        _analyticsService = analyticsService;
        _context = context;
    }

    [BindProperty(SupportsGet = true)]
    public Guid EventId { get; set; }

    public Event Event { get; set; } = null!;
    public Dictionary<string, double> AverageScores { get; set; } = new();
    public List<string> RecentComments { get; set; } = new();

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

        AverageScores = await _analyticsService.GetAverageScoresByEventAsync(EventId, cancellationToken);
        RecentComments = await _analyticsService.GetRecentCommentsByEventAsync(EventId, cancellationToken);

        return Page();
    }
}
