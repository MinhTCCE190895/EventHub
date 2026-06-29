using DAL.Interfaces;
using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories;

public class BookingRepository : BaseRepository<Booking>, IBookingRepository
{
    public BookingRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<bool> HasUserBookedEventAsync(Guid eventId, Guid studentId)
    {
        return await _dbSet.AnyAsync(b => b.EventId == eventId && b.StudentId == studentId && b.Status != "Cancelled");
    }
}
