using DAL.Entities;

namespace BLL.Services;

public interface IFeedbackService
{
    Task<bool> CanUserLeaveFeedbackAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default);
    Task SubmitFeedbackAsync(Guid userId, Guid eventId, string? generalComment, List<(string Criteria, int Score)> criteriaScores, CancellationToken cancellationToken = default);
}
