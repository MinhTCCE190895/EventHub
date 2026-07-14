using BusinessObjects.DTOs;

namespace BLL.Interfaces;

public interface IBookingService
{
    Task<string> BookTicketAsync(Guid eventId, Guid studentId);
    Task<IEnumerable<BookingDTO>> GetRecentBookingsAsync(int count, CancellationToken cancellationToken = default);
    Task<BookingDTO?> GetRecentBookingForEventAsync(Guid eventId, CancellationToken cancellationToken = default);
}
