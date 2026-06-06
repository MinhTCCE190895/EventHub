namespace DAL.Entities;

public class Bookmark
{
    public Guid StudentId { get; set; }
    public Guid EventId { get; set; }
    public DateTime SavedAt { get; set; }
    public string? Notes { get; set; }

    // Navigation properties
    public User Student { get; set; } = null!;
    public Event Event { get; set; } = null!;
}
