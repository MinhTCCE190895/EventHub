namespace BLL.Services;

public interface IEventReminderService
{
    /// Tìm kiếm và xử lý gửi email nhắc nhở cho tất cả các sự kiện sắp diễn ra đến hạn.
    Task ProcessPendingRemindersAsync(CancellationToken cancellationToken = default);
}
