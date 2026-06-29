using DAL.Entities;

namespace DAL.Interfaces;

public interface IBookingRepository : IRepository<Booking>
{
    Task<bool> HasUserBookedEventAsync(Guid eventId, Guid studentId);
}
