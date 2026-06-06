namespace DAL.Entities;

public class EventTag
{
    public Guid EventId { get; set; }
    public int TagId { get; set; }

    // Navigation properties
    public Event Event { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
