namespace DAL.Entities;

public class EventReminder
{
    public Guid EventId { get; set; }
    public DateTime ScheduledTime { get; set; }
    public bool IsEmailSent { get; set; } = false;
    public DateTime? SentAt { get; set; }

    // Navigation properties
    public Event Event { get; set; } = null!;

    // Calculated property to check if reminder can be sent
    public bool IsCanReminder => !IsEmailSent && Event != null && DateTime.UtcNow >= Event.StartTime.AddDays(-1) && DateTime.UtcNow < Event.StartTime;
}

