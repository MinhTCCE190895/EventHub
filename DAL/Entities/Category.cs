namespace DAL.Entities;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }

    // Navigation properties
    public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
}
