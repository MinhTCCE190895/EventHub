namespace BLL.Services;

public interface IEmailSender
{
    // Định nghĩa phương thức gửi email bất đồng bộ trong hệ thống.
    // Nhận vào địa chỉ người nhận, tiêu đề, nội dung email kèm token hủy tác vụ để lớp triển khai áp dụng.
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
}
