using DAL.Data;
using DAL.Repositories;
using Microsoft.Extensions.Logging;

namespace BLL.Services;

public class EventReminderService : IEventReminderService
{
    private readonly IEventReminderRepository _reminderRepository;
    private readonly IEmailSender _emailSender;
    private readonly AppDbContext _context;
    private readonly ILogger<EventReminderService> _logger;

    public EventReminderService(
        IEventReminderRepository reminderRepository,
        IEmailSender emailSender,
        AppDbContext context,
        ILogger<EventReminderService> logger)
    {
        _reminderRepository = reminderRepository;
        _emailSender = emailSender;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// LUỒNG HOẠT ĐỘNG CHÍNH (TPL Core Engine):
    /// 1. Gọi Repository để lấy ra tất cả EventReminders đến hạn gửi và chưa được xử lý (IsEmailSent == false).
    /// 2. Duyệt qua từng EventReminder.
    /// 3. Lọc ra danh sách Bookings của Event đó có trạng thái "Confirmed".
    /// 4. Sử dụng Parallel.ForEachAsync để bắt đầu xử lý đa luồng (TPL) gửi mail song song cho toàn bộ sinh viên đã đăng ký.
    /// 5. Sau khi hoàn tất tiến trình gửi mail của một sự kiện, cập nhật trạng thái IsEmailSent = true và SentAt = DateTime.UtcNow.
    /// 6. Gọi Repository để cập nhật thay đổi xuống Database.
    /// </summary>
    public async Task ProcessPendingRemindersAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        _logger.LogInformation(">>> [TPL Core Engine] Bắt đầu quét các email nhắc nhở lúc: {Time}", now);

        // Bước 1 & 2: Lấy dữ liệu qua Repository (Eager Loading tránh N+1 Query)
        var pendingReminders = await _reminderRepository.GetPendingRemindersWithDetailsAsync(now, cancellationToken);
        var remindersList = pendingReminders.ToList();

        if (!remindersList.Any())
        {
            _logger.LogInformation(">>> [TPL Core Engine] Không tìm thấy email nhắc nhở nào cần gửi.");
            return;
        }

        _logger.LogInformation(">>> [TPL Core Engine] Phát hiện {Count} sự kiện cần gửi email nhắc nhở.", remindersList.Count);

        foreach (var reminder in remindersList)
        {
            if (!reminder.IsCanReminder)
            {
                continue;
            }

            // Bước 3: Lọc danh sách đăng ký đã xác nhận (Confirmed)
            var activeBookings = reminder.Event.Bookings
                .Where(b => b.Status == "Confirmed")
                .ToList();

            if (activeBookings.Any())
            {
                _logger.LogInformation(">>> [TPL Core Engine] Đang xử lý gửi email cho sự kiện '{EventTitle}' tới {UserCount} sinh viên song song...",
                    reminder.Event.Title, activeBookings.Count);

                // Bước 4: Sử dụng Parallel.ForEachAsync để khởi chạy Task chạy song song
                // Tận dụng ThreadPool để xử lý nhiều yêu cầu I/O (gửi mail SMTP) đồng thời thay vì chạy tuần tự.
                await Parallel.ForEachAsync(activeBookings, new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = 10 // Giới hạn tối đa 10 luồng gửi song song cùng lúc để tránh làm nghẽn SMTP Server
                }, async (booking, ct) =>
                {
                    try
                    {
                        var studentEmail = booking.Student.Email;
                        var subject = $"[UniEvent Hub] Nhắc nhở sự kiện sắp diễn ra: {reminder.Event.Title}";
                        var body = $@"
<div style=""font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e0e0e0; border-radius: 8px; background-color: #f9f9f9; color: #333333;"">
    
    <div style=""text-align: center; border-bottom: 2px solid #0056b3; padding-bottom: 15px; margin-bottom: 20px;"">
        <h2 style=""color: #0056b3; margin: 0; font-size: 24px;"">UniEvent Hub</h2>
        <p style=""font-size: 14px; color: #666666; margin: 5px 0 0 0;"">Nền tảng Quản lý và Tham gia Sự kiện Sinh viên</p>
    </div>

    <div>
        <p style=""font-size: 16px; line-height: 1.5;"">Chào <strong>{booking.Student.FullName}</strong>,</p>
        
        <p style=""font-size: 15px; line-height: 1.5;"">
            Đây là thư nhắc nhở từ hệ thống. Sự kiện mà bạn đăng ký tham gia đã sắp đến giờ khởi chạy. Dưới đây là thông tin chi tiết:
        </p>

        <div style=""background-color: #ffffff; border-left: 4px solid #0056b3; padding: 15px; margin: 20px 0; border-radius: 0 8px 8px 0; box-shadow: 0 2px 4px rgba(0,0,0,0.05);"">
            <h3 style=""margin-top: 0; color: #111111; font-size: 18px;"">🎬 {reminder.Event.Title}</h3>
            <p style=""margin: 8px 0; font-size: 15px;"">
                🗓️ <strong>Thời gian bắt đầu:</strong> <span style=""color: #d9534f; font-weight: bold;"">{reminder.Event.StartTime.ToLocalTime():dd/MM/yyyy HH:mm}</span>
            </p>
            <p style=""margin: 8px 0; font-size: 15px;"">
                🎟️ <strong>Mã vé của bạn:</strong> <span style=""background-color: #eef2f7; padding: 3px 8px; border-radius: 4px; font-family: monospace; font-size: 16px; font-weight: bold; color: #2c3e50; border: 1px dashed #b0c4de;"">{booking.TicketCode}</span>
            </p>
        </div>

        <p style=""font-size: 15px; line-height: 1.5;"">
            Bạn vui lòng đến sớm trước 15 phút và xuất trình <strong>Mã vé</strong> này tại bàn check-in để ban tổ chức hỗ trợ bạn tốt nhất nhé.
        </p>
        
        <p style=""font-size: 15px; font-style: italic; font-weight: bold; color: #2ecc71; margin-top: 25px;"">
            🌟 Chúc bạn có một trải nghiệm thật tuyệt vời tại sự kiện!
        </p>
    </div>

    <div style=""margin-top: 30px; padding-top: 15px; border-top: 1px solid #e0e0e0; text-align: center; font-size: 13px; color: #777777;"">
        <p style=""margin: 0 0 5px 0;"">Trân trọng,</p>
        <p style=""margin: 0; font-weight: bold; color: #0056b3;"">Ban Tổ Chức UniEvent Hub</p>
        <p style=""margin: 10px 0 0 0; font-size: 11px; color: #aaaaaa;"">Đây là email tự động từ hệ thống, vui lòng không phản hồi lại email này.</p>
    </div>

</div>
";
                        await _emailSender.SendEmailAsync(studentEmail, subject, body, ct);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ">>> [TPL Core Engine] Lỗi khi gửi email cho sinh viên ở Booking Id: {BookingId}.", booking.Id);
                    }
                });
            }

            // Bước 5 & 6: Cập nhật trạng thái và lưu lại
            reminder.IsEmailSent = true;
            reminder.SentAt = DateTime.UtcNow;
            _reminderRepository.Update(reminder);
        }

        // Lưu tất cả cập nhật trạng thái reminders vào Database
        await _context.SaveChangesAsync(cancellationToken);
    }
}
