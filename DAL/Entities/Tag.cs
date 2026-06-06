namespace DAL.Entities;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    // Navigation properties
    public ICollection<EventTag> EventTags { get; set; } = new List<EventTag>();
}
