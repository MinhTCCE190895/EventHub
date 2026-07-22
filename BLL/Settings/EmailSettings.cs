using System.ComponentModel.DataAnnotations;

namespace BLL.Settings;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";

    [Required]
    public string SmtpHost { get; set; } = string.Empty;

    [Range(1, 65535)]
    public int SmtpPort { get; set; }

    public bool EnableSsl { get; set; }

    [Required]
    [EmailAddress]
    public string SenderEmail { get; set; } = string.Empty;

    [Required]
    public string SenderName { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
