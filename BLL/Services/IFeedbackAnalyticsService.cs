using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL.Services;

public interface IFeedbackAnalyticsService
{
    Task<Dictionary<string, double>> GetAverageScoresByEventAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<List<string>> GetRecentCommentsByEventAsync(Guid eventId, CancellationToken cancellationToken = default);
}
