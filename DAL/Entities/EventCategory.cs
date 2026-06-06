namespace DAL.Entities;

public class EventCategory
{
    public Guid EventId { get; set; }
    public int CategoryId { get; set; }

    // Navigation properties
    public Event Event { get; set; } = null!;
    public Category Category { get; set; } = null!;
}
