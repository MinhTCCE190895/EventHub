using BLL.SignalR;
using DAL.Data;
using DAL.Entities;
using DAL.Repositories;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepo;
    private readonly IEventRepository _eventRepo;
    private readonly AppDbContext _context;
    private readonly IHubContext<EventHub> _hubContext;

    public BookingService(
        IBookingRepository bookingRepo,
        IEventRepository eventRepo,
        AppDbContext context,
        IHubContext<EventHub> hubContext)
    {
        _bookingRepo = bookingRepo;
        _eventRepo = eventRepo;
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<string> BookTicketAsync(Guid eventId, Guid studentId)
    {
        // 1. Kiểm tra đã đặt vé chưa
        if (await _bookingRepo.HasUserBookedEventAsync(eventId, studentId))
        {
            throw new InvalidOperationException("User has already booked this event.");
        }

        // 2. Lấy event để kiểm tra capacity
        var evWithVenue = await _context.Events
            .Include(e => e.Venue)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (evWithVenue == null)
            throw new KeyNotFoundException("Event not found.");

        if (evWithVenue.RegisteredCount >= evWithVenue.Venue.MaxCapacity)
        {
            throw new InvalidOperationException("Sự kiện đã hết vé.");
        }

        // 3. Thực hiện tạo Booking
        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            StudentId = studentId,
            TicketCode = $"TKT-{eventId.ToString().Substring(0, 4)}-{Guid.NewGuid().ToString().Substring(0, 4)}",
            BookingTime = DateTime.UtcNow,
            Status = "Confirmed",
            IsCheckedIn = false
        };

        await _bookingRepo.AddAsync(booking);

        // 4. Cập nhật RegisteredCount của Event
        evWithVenue.RegisteredCount += 1;
        _eventRepo.Update(evWithVenue);

        // 5. Lưu xuống DB - Bắt lỗi DbUpdateConcurrencyException
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new DbUpdateConcurrencyException("Có người khác đã đặt vé cùng lúc, vui lòng thử lại.", ex);
        }

        // 6. Push real-time update qua SignalR
        int remainingSeats = evWithVenue.Venue.MaxCapacity - evWithVenue.RegisteredCount;
        await _hubContext.Clients.All.SendAsync("ReceiveTicketUpdate", eventId, remainingSeats);

        return booking.TicketCode;
    }
}
