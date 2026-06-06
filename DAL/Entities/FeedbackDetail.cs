namespace DAL.Entities;

public class FeedbackDetail
{
    public int Id { get; set; }
    public Guid FeedbackId { get; set; }
    public string Criteria { get; set; } = null!;
    public int Score { get; set; }

    // Navigation properties
    public Feedback Feedback { get; set; } = null!;
}
