using DAL.Entities;

namespace DAL.Interfaces;

public interface IEventReminderRepository : IRepository<EventReminder>
{
    /// <summary>
    /// Lấy danh sách các nhắc nhở sự kiện chưa gửi và đã đến hoặc quá thời gian gửi được lên lịch.
    /// </summary>
    Task<IEnumerable<EventReminder>> GetPendingRemindersWithDetailsAsync(DateTime now, CancellationToken cancellationToken = default);
}
