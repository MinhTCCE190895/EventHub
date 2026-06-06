namespace DAL.Entities;

public class Feedback
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public string? GeneralComment { get; set; }
    public DateTime SubmittedAt { get; set; }

    // Navigation properties
    public Booking Booking { get; set; } = null!;
    public ICollection<FeedbackDetail> FeedbackDetails { get; set; } = new List<FeedbackDetail>();
}
