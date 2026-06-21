using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs;

public class OrganizerCreateDTO
{
    [Required(ErrorMessage = "Full Name is required")]
    [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 đến 100 ký tự")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Xác nhận mật khẩu là bắt buộc")]
    [Compare("Password", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp")]
    [DataType(DataType.Password)]
    public string ConfirmPassword { get; set; } = null!;

    [Display(Name = "Student Code")]
    public string? StudentCode { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    [Display(Name = "Email")]
    public string Email { get; set; } = null!;
}
