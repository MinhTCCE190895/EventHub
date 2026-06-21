using System.ComponentModel.DataAnnotations;

namespace DAL.Entities;

public class Event
{
    public Guid Id { get; set; }
    public Guid OrganizerId { get; set; }
    public int VenueId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string BannerUrl { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public int RegisteredCount { get; set; } = 0;

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

    // Navigation properties
    public User Organizer { get; set; } = null!;
    public Venue Venue { get; set; } = null!;
    public ICollection<EventCategory> EventCategories { get; set; } = new List<EventCategory>();
    public ICollection<EventTag> EventTags { get; set; } = new List<EventTag>();
    public ICollection<Bookmark> Bookmarks { get; set; } = new List<Bookmark>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<EventComment> EventComments { get; set; } = new List<EventComment>();
    public EventReminder? EventReminder { get; set; }
}
