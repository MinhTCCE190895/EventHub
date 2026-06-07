namespace BusinessObjects.DTOs;

public class EventCardDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? BannerUrl { get; set; }
    public DateTime StartTime { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public List<string> TagNames { get; set; } = new();
}
