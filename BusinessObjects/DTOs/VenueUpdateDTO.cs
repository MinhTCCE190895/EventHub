using System.ComponentModel.DataAnnotations;

namespace BusinessObjects.DTOs;

public class VenueUpdateDTO
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Venue Name is required")]
    [MaxLength(200, ErrorMessage = "Venue Name cannot exceed 200 characters")]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = "Max Capacity is required")]
    [Range(1, 100000, ErrorMessage = "Capacity must be between 1 and 100,000")]
    public int MaxCapacity { get; set; }

    [Required(ErrorMessage = "Address is required")]
    [MaxLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string Address { get; set; } = null!;

    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }
}
