namespace DAL.Entities;

public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string? StudentCode { get; set; }
    public string Role { get; set; } = null!;
    public bool IsActive { get; set; } = true;
    public string Email { get; set; } = null!;
    public string? AvatarUrl { get; set; }
    public string PasswordHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    // Navigation properties
    public ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();
    public ICollection<EventRequest> EventRequests { get; set; } = new List<EventRequest>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<EventComment> EventComments { get; set; } = new List<EventComment>();
    public ICollection<Follow> Followers { get; set; } = new List<Follow>();
    public ICollection<Follow> Followees { get; set; } = new List<Follow>();
}
