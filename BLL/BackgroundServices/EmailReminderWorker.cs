using BLL.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BLL.BackgroundServices;

// Chạy ngầm định kỳ để tự động kích hoạt tiến trình gửi email nhắc nhở sự kiện.
// Khởi chạy vòng lặp vô hạn song song với ứng dụng -> Gọi hàm xử lý chính -> Tạm dừng 15 giây trước khi lặp lại vòng quét tiếp theo.
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

    // Tạo phạm vi dịch vụ (Scope) để gọi tầng nghiệp vụ xử lý gửi email.
    // Khởi tạo Dependency Injection Scope -> Giải mã dịch vụ IEventReminderService -> Gọi hàm xử lý các reminder đang chờ gửi.
    private async Task SendPendingRemindersAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var reminderService = scope.ServiceProvider.GetRequiredService<IEventReminderService>();
        await reminderService.ProcessPendingRemindersAsync(stoppingToken);
    }
}
