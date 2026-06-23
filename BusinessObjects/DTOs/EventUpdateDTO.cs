using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs;

public class EventUpdateDTO
{
    [Required]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Tiêu đề không được để trống.")]
    [MaxLength(200)]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "Mô tả không được để trống.")]
    public string Description { get; set; } = null!;

    public string BannerUrl { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu.")]
    public DateTime StartTime { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn ngày kết thúc.")]
    public DateTime EndTime { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn địa điểm.")]
    public int VenueId { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn nhà tổ chức.")]
    public Guid OrganizerId { get; set; }

    public List<int> CategoryIds { get; set; } = new();
    public List<int> TagIds { get; set; } = new();
}
