using System.ComponentModel.DataAnnotations;

namespace BLL.DTOs;

public class VenueDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int MaxCapacity { get; set; }
    public string Address { get; set; } = null!;
    public string? Description { get; set; }
}
