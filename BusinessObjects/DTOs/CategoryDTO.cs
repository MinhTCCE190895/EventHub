using System;
using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs;

public class CategoryDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
}
