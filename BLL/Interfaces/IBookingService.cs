namespace BLL.Interfaces;

public interface IBookingService
{
    Task<string> BookTicketAsync(Guid eventId, Guid studentId);
}
