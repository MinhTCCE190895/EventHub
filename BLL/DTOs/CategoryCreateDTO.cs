using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs;

public class CategoryCreateDTO
{
    [Required(ErrorMessage = "Category Name is required")]
    [MaxLength(100, ErrorMessage = "Category Name cannot exceed 100 characters")]
    public string Name { get; set; } = null!;

    [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }
}
