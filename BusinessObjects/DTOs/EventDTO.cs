namespace BusinessObjects.DTOs;

public class EventDTO
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string BannerUrl { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = string.Empty;

    public string StatusDisplayName => Status switch
    {
        "Draft" => "Bản nháp",
        "Published" => "Đã xuất bản",
        "Cancelled" => "Đã hủy",
        _ => Status
    };

    public string StatusBadgeClass => Status switch
    {
        "Draft" => "bg-secondary",
        "Published" => "bg-success",
        "Cancelled" => "bg-danger",
        _ => "bg-light text-dark"
    };
    public int RegisteredCount { get; set; }
    public Guid OrganizerId { get; set; }
    public string OrganizerName { get; set; } = string.Empty;
    public int VenueId { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public int VenueMaxCapacity { get; set; }

    public List<int> CategoryIds { get; set; } = new();
    public List<int> TagIds { get; set; } = new();
}
