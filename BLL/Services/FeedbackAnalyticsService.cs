using DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Services;

public class FeedbackAnalyticsService : IFeedbackAnalyticsService
{
    private readonly IFeedbackRepository _feedbackRepository;

    public FeedbackAnalyticsService(IFeedbackRepository feedbackRepository)
    {
        _feedbackRepository = feedbackRepository;
    }

    public async Task<Dictionary<string, double>> GetAverageScoresByEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var feedbacks = await _feedbackRepository.GetFeedbacksByEventIdAsync(eventId, cancellationToken);
        var feedbacksList = feedbacks.ToList();

        if (!feedbacksList.Any())
        {
            return new Dictionary<string, double>();
        }

        // Flatten all feedback details
        var allDetails = feedbacksList.SelectMany(f => f.FeedbackDetails).ToList();

        if (!allDetails.Any())
        {
            return new Dictionary<string, double>();
        }

        // FE-07: PLINQ (.AsParallel()) to calculate multi-criteria average scores concurrently
        var averageScores = allDetails
            .AsParallel()
            .GroupBy(d => d.Criteria)
            .ToDictionary(
                g => g.Key,
                g => Math.Round(g.Average(d => d.Score), 2)
            );

        return averageScores;
    }

    public async Task<List<string>> GetRecentCommentsByEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var feedbacks = await _feedbackRepository.GetFeedbacksByEventIdAsync(eventId, cancellationToken);
        
        return feedbacks
            .Where(f => !string.IsNullOrWhiteSpace(f.GeneralComment))
            .OrderByDescending(f => f.SubmittedAt)
            .Select(f => f.GeneralComment!)
            .ToList();
    }
}
