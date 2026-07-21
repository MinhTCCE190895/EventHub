namespace DAL.Entities;

public class EventComment
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public bool IsHidden { get; set; } = false;

    // Navigation properties
    public Event Event { get; set; } = null!;
    public User User { get; set; } = null!;
    public EventComment? ParentComment { get; set; }
    public ICollection<EventComment> Replies { get; set; } = new List<EventComment>();
}
