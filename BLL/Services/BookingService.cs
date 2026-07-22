using BLL.Interfaces;
using BLL.SignalR;
using DAL.Data;
using DAL.Entities;
using DAL.Repositories;
using DAL.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BLL.DTOs;

namespace BLL.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _bookingRepo;
    private readonly IEventRepository _eventRepo;
    private readonly AppDbContext _context;
    private readonly IHubContext<EventHub> _hubContext;
    private readonly IMapper _mapper;

    public BookingService(
        IBookingRepository bookingRepo,
        IEventRepository eventRepo,
        AppDbContext context,
        IHubContext<EventHub> hubContext,
        IMapper mapper)
    {
        _bookingRepo = bookingRepo;
        _eventRepo = eventRepo;
        _context = context;
        _hubContext = hubContext;
        _mapper = mapper;
    }

    public async Task<string> BookTicketAsync(Guid eventId, Guid studentId)
    {
        // Thiết lập database transaction Serializable để cô lập giao dịch hoàn toàn chống race condition
        using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        try
        {
            // 1. Kiểm tra đã đặt vé chưa
            if (await _bookingRepo.HasUserBookedEventAsync(eventId, studentId))
            {
                throw new InvalidOperationException("Bạn đã đặt vé cho sự kiện này rồi.");
            }

            // 2. Lấy event để kiểm tra capacity và thời gian kết thúc
            var evWithVenue = await _context.Events
                .Include(e => e.Venue)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (evWithVenue == null)
                throw new KeyNotFoundException("Không tìm thấy sự kiện.");

            if (evWithVenue.EndTime < DateTime.UtcNow)
            {
                throw new InvalidOperationException("Sự kiện đã kết thúc, không thể đặt vé.");
            }

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

            // 5. Lưu xuống DB
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // 6. Push real-time update qua SignalR
            int remainingSeats = evWithVenue.Venue.MaxCapacity - evWithVenue.RegisteredCount;
            await _hubContext.Clients.All.SendAsync("ReceiveTicketUpdate", eventId, remainingSeats);

            return booking.TicketCode;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync();
            throw new InvalidOperationException("Vé sự kiện đang được đặt tranh chấp bởi người dùng khác. Vui lòng tải lại trang và thử lại.", ex);
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync();
            // Bắt lỗi vi phạm Unique Index do đặt vé song song (Multi-tab)
            if (ex.InnerException?.Message.Contains("UNIQUE") == true || ex.InnerException?.Message.Contains("Cannot insert duplicate key") == true)
            {
                throw new InvalidOperationException("Hệ thống phát hiện bạn đang thực hiện đặt vé trên một tab khác hoặc đã sở hữu vé.", ex);
            }
            throw;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<BookingDTO?> GetUserBookingForEventAsync(Guid eventId, Guid studentId, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepo.Query()
            .Where(b => b.EventId == eventId && b.StudentId == studentId && b.Status == "Confirmed")
            .Include(b => b.Student)
            .Include(b => b.Event)
            .FirstOrDefaultAsync(cancellationToken);

        return booking == null ? null : _mapper.Map<BookingDTO>(booking);
    }

    public async Task<IEnumerable<BookingDTO>> GetRecentBookingsAsync(int count, CancellationToken cancellationToken = default)
    {
        var bookings = await _bookingRepo.Query()
            .Where(b => b.Status == "Confirmed")
            .Include(b => b.Student)
            .Include(b => b.Event)
            .OrderByDescending(b => b.BookingTime)
            .Take(count)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<BookingDTO>>(bookings);
    }

    public async Task<BookingDTO?> GetRecentBookingForEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepo.Query()
            .Where(b => b.EventId == eventId && b.Status == "Confirmed")
            .Include(b => b.Student)
            .Include(b => b.Event)
            .OrderByDescending(b => b.BookingTime)
            .FirstOrDefaultAsync(cancellationToken);

        return booking == null ? null : _mapper.Map<BookingDTO>(booking);
    }
}
