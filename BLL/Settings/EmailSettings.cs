namespace BLL.Settings;

// Chứa các cấu hình kết nối phục vụ cho việc gửi Email qua giao thức SMTP.
// Ánh xạ dữ liệu cấu hình từ file appsettings.json vào các thuộc tính để sử dụng thông qua IOptions.
public class EmailSettings
{
    public string SmtpHost { get; set; } = null!;
    public int SmtpPort { get; set; }
    public bool EnableSsl { get; set; }
    public string SenderEmail { get; set; } = null!;
    public string SenderName { get; set; } = null!;
    public string Password { get; set; } = null!;
}
