namespace DAL.Entities;

public class Venue
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int MaxCapacity { get; set; }
    public string Address { get; set; } = null!;
    public string? Description { get; set; }

    // Navigation properties
    public ICollection<Event> Events { get; set; } = new List<Event>();
}
