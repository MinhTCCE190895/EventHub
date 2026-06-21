namespace BLL.Services;

public interface IBookingService
{
    Task<string> BookTicketAsync(Guid eventId, Guid studentId);
}
