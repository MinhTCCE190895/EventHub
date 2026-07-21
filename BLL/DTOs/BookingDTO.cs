namespace BLL.DTOs;

public class BookingDTO
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public string EventTitle { get; set; } = string.Empty;
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string TicketCode { get; set; } = string.Empty;
    public DateTime BookingTime { get; set; }
    public string Status { get; set; } = string.Empty;
}
