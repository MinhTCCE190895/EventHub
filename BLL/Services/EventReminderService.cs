using BLL.Interfaces;
using DAL.Data;
using DAL.Repositories;
using DAL.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Data;

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

    // Quét và xử lý gửi email nhắc nhở cho các sự kiện sắp diễn ra.
    // Mở Transaction mức Serializable -> Lấy các reminder chưa gửi -> Cập nhật trạng thái đã gửi trong DB -> Commit -> Dùng Parallel.ForEachAsync gửi email song song hàng loạt.
    public async Task ProcessPendingRemindersAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        // log debug để biết lúc nó đang hoạt động
        _logger.LogInformation(">>> [TPL Core Engine] Bắt đầu quét các email nhắc nhở lúc: {Time}", now);

        List<DAL.Entities.EventReminder> remindersList;

        // Sử dụng Transaction với mức cô lập Serializable để tránh tranh chấp (Race Condition) giữa nhiều instance của Worker
        using (var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken))
        {
            try
            {
                var pendingReminders = await _reminderRepository.GetPendingRemindersWithDetailsAsync(now, cancellationToken);
                remindersList = pendingReminders.ToList();

                if (!remindersList.Any())
                {// log debug ko tìm thấy sự kiện cần gửi mail
                    _logger.LogInformation(">>> [TPL Core Engine] Không tìm thấy sự kiện nào cần gửi mail nhắc nhở.");
                    await transaction.CommitAsync(cancellationToken);
                    return;
                }

                // Cập nhật trạng thái đã gửi/đang gửi ngay lập tức rồi lưu thay đổi và commit trước khi tiến hành gửi mail
                foreach (var reminder in remindersList)
                {
                    reminder.IsEmailSent = true;
                    reminder.SentAt = now;
                    _reminderRepository.Update(reminder);
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                // log debug báo lỗi
                _logger.LogError(ex, ">>> [TPL Core Engine] Gặp lỗi khi khóa/cập nhật trạng thái reminders. Tiến hành rollback.");
                await transaction.RollbackAsync(cancellationToken);
                return;
            }
        }
        // log debug xem có bao nhiêu sự kiện cần gửi mail.
        _logger.LogInformation(">>> [TPL Core Engine] Phát hiện {Count} sự kiện cần gửi email nhắc nhở.", remindersList.Count);

        foreach (var reminder in remindersList)
        {   // log debug xem tên sự kiện, id và số lượt book
            var rawBookingsCount = reminder.Event?.Bookings?.Count ?? 0;
            _logger.LogInformation(">>> [DEBUG] Sự kiện '{EventTitle}' (Id: {EventId}) - Tổng số Bookings trong RAM: {RawCount}",
                reminder.Event?.Title, reminder.EventId, rawBookingsCount);

            if (reminder.Event?.Bookings != null)
            {
                foreach (var b in reminder.Event.Bookings)
                {   // log debug sinh viên book
                    _logger.LogInformation(">>> [DEBUG] Booking Id: {BookingId} | Status: '{Status}' | StudentEmail: '{Email}'",
                        b.Id, b.Status, b.Student?.Email);
                }
            }

            var activeBookings = reminder.Event.Bookings
                .Where(b => b.Status == "Confirmed")
                .ToList();

            if (activeBookings.Any())
            {   // log debug thông báo gủi thành công đến từng sv
                _logger.LogInformation(">>> [TPL Core Engine] Đang xử lý gửi email cho sự kiện '{EventTitle}' tới {UserCount} sinh viên song song...",
                    reminder.Event.Title, activeBookings.Count);

                await Parallel.ForEachAsync(activeBookings, new ParallelOptions
                {
                    CancellationToken = cancellationToken,
                    MaxDegreeOfParallelism = 10
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
                    {   // log debug Gửi ko thành công.
                        _logger.LogError(ex, ">>> [TPL Core Engine] Lỗi khi gửi email cho sinh viên ở Booking Id: {BookingId}.", booking.Id);
                    }
                });
            }
        }
    }
}
