using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class FeedbackRepository : BaseRepository<Feedback>, IFeedbackRepository
{
    public FeedbackRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Feedback>> GetFeedbacksByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.FeedbackDetails)
            .Include(f => f.Booking)
                .ThenInclude(b => b.Student)
            .Where(f => f.Booking.EventId == eventId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
