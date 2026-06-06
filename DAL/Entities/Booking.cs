namespace DAL.Entities;

public class Booking
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid StudentId { get; set; }
    public string TicketCode { get; set; } = null!;
    public DateTime BookingTime { get; set; }
    public string Status { get; set; } = null!;
    public bool IsCheckedIn { get; set; } = false;

    // Navigation properties
    public Event Event { get; set; } = null!;
    public User Student { get; set; } = null!;
    public Feedback? Feedback { get; set; }
}
