namespace DAL.Entities;

public class EventRequest
{
    public int Id { get; set; }
    public Guid StudentId { get; set; }
    public string Topic { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Status { get; set; } = "Pending";
    public DateTime SubmittedAt { get; set; }
    public string? ResponseMessage { get; set; }

    // Navigation properties
    public User Student { get; set; } = null!;
}
