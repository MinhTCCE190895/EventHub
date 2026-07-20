using System;
using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs;

public class CategoryDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}
