namespace BLL.Services;

public interface IEventReminderService
{
    /// <summary>
    /// Tìm kiếm và xử lý gửi email nhắc nhở cho tất cả các sự kiện sắp diễn ra đến hạn.
    /// </summary>
    Task ProcessPendingRemindersAsync(CancellationToken cancellationToken = default);
}
