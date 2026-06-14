using BLL.Services;
using DAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BLL.BackgroundServices;

public class EmailReminderWorker : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailReminderWorker> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(15); // Quét định kỳ mỗi 15 giây để dễ quan sát log khi chạy thử

    public EmailReminderWorker(IServiceProvider serviceProvider, ILogger<EmailReminderWorker> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("EmailReminderWorker has started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendPendingRemindersAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while executing EmailReminderWorker.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("EmailReminderWorker is stopping.");
    }

    private async Task SendPendingRemindersAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var now = DateTime.UtcNow;

        // Quét các EventReminder đến hạn gửi và chưa gửi
        var pendingReminders = await context.EventReminders
            .Include(er => er.Event)
                .ThenInclude(e => e.Bookings)
                    .ThenInclude(b => b.Student)
            .Where(er => !er.IsEmailSent && er.ScheduledTime <= now)
            .ToListAsync(stoppingToken);

        if (!pendingReminders.Any()) return;

        _logger.LogInformation("Found {Count} pending event reminders to send.", pendingReminders.Count);

        foreach (var reminder in pendingReminders)
        {
            var activeBookings = reminder.Event.Bookings
                .Where(b => b.Status == "Confirmed")
                .ToList();

            if (activeBookings.Any())
            {
                _logger.LogInformation("Sending reminder for event '{EventTitle}' to {UserCount} students in parallel.",
                    reminder.Event.Title, activeBookings.Count);

                // FE-06: Sử dụng TPL Parallel.ForEachAsync để gửi email song song
                await Parallel.ForEachAsync(activeBookings, new ParallelOptions
                {
                    CancellationToken = stoppingToken,
                    MaxDegreeOfParallelism = 10 // Giới hạn tối đa 10 tác vụ chạy song song cùng lúc
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
                        await emailSender.SendEmailAsync(studentEmail, subject, body, ct);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send email to booking {BookingId}.", booking.Id);
                    }
                });
            }

            // Đánh dấu đã gửi
            reminder.IsEmailSent = true;
            reminder.SentAt = DateTime.UtcNow;
        }

        // Lưu lại cập nhật trạng thái reminders
        await context.SaveChangesAsync(stoppingToken);
    }
}
