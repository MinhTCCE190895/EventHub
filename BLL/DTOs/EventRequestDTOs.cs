using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs;

public class EventRequestDTO
{
    public int Id { get; set; }
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string Topic { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Status { get; set; } = "Pending";
    public DateTime SubmittedAt { get; set; }
    public string? ResponseMessage { get; set; }

    public string StatusDisplayName => Status switch
    {
        "Approved" => "Đã duyệt",
        "Rejected" => "Từ chối",
        "Pending" => "Chờ duyệt",
        _ => Status ?? "Chờ duyệt"
    };

    public string StatusBadgeClass => Status switch
    {
        "Approved" => "bg-success-subtle text-success border border-success",
        "Rejected" => "bg-danger-subtle text-danger border border-danger",
        "Pending" => "bg-warning-subtle text-warning border border-warning",
        _ => "bg-warning-subtle text-warning border border-warning"
    };

    public string StatusIconClass => Status switch
    {
        "Approved" => "bi-check-circle-fill",
        "Rejected" => "bi-x-circle-fill",
        "Pending" => "bi-hourglass-split",
        _ => "bi-hourglass-split"
    };
}

public class EventRequestCreateDTO
{
    [Required(ErrorMessage = "Vui lòng nhập chủ đề ý tưởng.")]
    [MaxLength(200, ErrorMessage = "Chủ đề tối đa 200 ký tự.")]
    public string Topic { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng nhập mô tả chi tiết ý tưởng.")]
    [MaxLength(2000, ErrorMessage = "Mô tả tối đa 2000 ký tự.")]
    public string Description { get; set; } = null!;
}

public class EventRequestProcessDTO
{
    [Required]
    public int Id { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn trạng thái phê duyệt.")]
    public string Status { get; set; } = null!;

    [MaxLength(500, ErrorMessage = "Phản hồi tối đa 500 ký tự.")]
    public string? ResponseMessage { get; set; }
}