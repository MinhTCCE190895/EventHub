namespace DAL.Entities;

public class EventReminder
{
    public Guid EventId { get; set; }
    public DateTime ScheduledTime { get; set; }
    public bool IsEmailSent { get; set; } = false;
    public DateTime? SentAt { get; set; }

    // Navigation properties
    public Event Event { get; set; } = null!;
}
