using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class EventReminderRepository : BaseRepository<EventReminder>, IEventReminderRepository
{
    // Inject AppDbContext vào qua Constructor của BaseRepository
    public EventReminderRepository(AppDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Luồng hoạt động:
    /// 1. Query từ bảng DbSet<EventReminder>.
    /// 2. Sử dụng .Include và .ThenInclude để tải trước (Eager Loading) thông tin Event, Bookings, và Student đăng ký để tránh lỗi N+1 Query.
    /// 3. Lọc ra những bản ghi chưa gửi (IsEmailSent == false) và ScheduledTime nhỏ hơn hoặc bằng thời gian hiện tại.
    /// </summary>
    public async Task<IEnumerable<EventReminder>> GetPendingRemindersWithDetailsAsync(DateTime now, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(er => er.Event)
                .ThenInclude(e => e.Bookings)
                    .ThenInclude(b => b.Student)
            .Where(er => !er.IsEmailSent && er.Event.StartTime.AddDays(-1) <= now && now < er.Event.StartTime)
            .ToListAsync(cancellationToken);
    }
}