using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class FeedbackRepository : BaseRepository<Feedback>, IFeedbackRepository
{
    public FeedbackRepository(AppDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Luồng hoạt động:
    /// 1. Query DbSet<Feedback> nạp thông tin chi tiết các tiêu chí đánh giá (FeedbackDetails).
    /// 2. Include thông tin Booking tương ứng để lọc theo EventId.
    /// </summary>
    public async Task<IEnumerable<Feedback>> GetFeedbacksByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.FeedbackDetails)
            .Include(f => f.Booking)
            .Where(f => f.Booking.EventId == eventId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Luồng hoạt động:
    /// 1. Query toàn bộ các đánh giá.
    /// 2. Nạp kèm FeedbackDetails cùng Booking và Event liên đới để tính toán tổng hợp cho Dashboard của Admin.
    /// </summary>
    public async Task<IEnumerable<Feedback>> GetAllFeedbacksWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(f => f.FeedbackDetails)
            .Include(f => f.Booking)
                .ThenInclude(b => b.Event)
            .ToListAsync(cancellationToken);
    }
}
