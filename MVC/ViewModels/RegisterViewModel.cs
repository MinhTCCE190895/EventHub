using System.ComponentModel.DataAnnotations;

namespace MVC.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Họ tên không được để trống.")]
    [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự.")]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Email không được để trống.")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [Display(Name = "Email")]
    public string Email { get; set; } = null!;

    [StringLength(20)]
    [Display(Name = "Mã sinh viên (nếu có)")]
    public string? StudentCode { get; set; }

    [Required(ErrorMessage = "Mật khẩu không được để trống.")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu tối thiểu 6 ký tự.")]
    [DataType(DataType.Password)]
    [Display(Name = "Mật khẩu")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Xác nhận mật khẩu không được để trống.")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
    [Display(Name = "Xác nhận mật khẩu")]
    public string ConfirmPassword { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng chọn vai trò.")]
    [Display(Name = "Vai trò")]
    public string Role { get; set; } = "Student";
}
