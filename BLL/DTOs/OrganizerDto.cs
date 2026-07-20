using System;

namespace BLL.DTOs;

public class OrganizerDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int FollowCount { get; set; }
    public bool IsFollowed { get; set; }
}
