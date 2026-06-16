using DAL.Entities;

namespace DAL.Repositories;

public interface IBookingRepository : IRepository<Booking>
{
    Task<bool> HasUserBookedEventAsync(Guid eventId, Guid studentId);
}
