using System;

namespace BusinessObjects.DTOs;

public class OrganizerDTO
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string? StudentCode { get; set; }
    public string Email { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
