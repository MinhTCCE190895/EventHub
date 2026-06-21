using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs;

public class OrganizerUpdateDTO
{
    [Required]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Full Name is required")]
    [StringLength(100, ErrorMessage = "Full Name cannot exceed 100 characters")]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = null!;

    [Display(Name = "Student Code")]
    public string? StudentCode { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid Email Address")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    [Display(Name = "Email")]
    public string Email { get; set; } = null!;

    [Display(Name = "Is Active")]
    public bool IsActive { get; set; }
}
