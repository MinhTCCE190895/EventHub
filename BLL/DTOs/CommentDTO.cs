namespace BLL.DTOs;

public class CommentDTO
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public string UserAvatarUrl { get; set; } = string.Empty;
    public string UserRole { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public Guid? ParentCommentId { get; set; }
    public List<CommentDTO> Replies { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public bool IsHidden { get; set; }

    public string RoleBadgeText => UserRole switch
    {
        "Admin" => "Admin",
        "Organizer" => "Ban Tổ Chức",
        _ => "Sinh Viên"
    };

    public string RoleBadgeStyle => UserRole switch
    {
        "Admin" => "background-color: #dc3545; color: #000000; font-weight: 600;",
        "Organizer" => "background-color: #fd7e14; color: #000000; font-weight: 600;",
        _ => "background-color: #e2e8f0; color: #000000; font-weight: 600;"
    };
}
