namespace BLL.DTOs;

public class FeedbackSubmissionDto
{
    public Guid BookingId { get; set; }
    public string? GeneralComment { get; set; }
    public List<FeedbackDetailDto> Details { get; set; } = new();
}

public class FeedbackDetailDto
{
    public string Criteria { get; set; } = null!;
    public int Score { get; set; }
}

public class EventFeedbackMetricsDto
{
    public Guid EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public int TotalFeedbacks { get; set; }
    public double OverallAverageScore { get; set; }
    public Dictionary<string, double> CriteriaAverages { get; set; } = new();
}
