using DAL.Entities;

namespace DAL.Repositories;

public interface IFeedbackRepository : IRepository<Feedback>
{
    Task<IEnumerable<Feedback>> GetFeedbacksByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
}
