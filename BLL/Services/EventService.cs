using BLL.Interfaces;
using AutoMapper;
using BusinessObjects.DTOs;
using DAL.Data;
using DAL.Entities;
using DAL.Repositories;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Microsoft.AspNetCore.SignalR;
using BLL.SignalR;

namespace BLL.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IRepository<User> _userRepository;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly IHubContext<EventHub> _hubContext;

    public EventService(
        IEventRepository eventRepository,
        IRepository<User> userRepository,
        AppDbContext context,
        IMapper mapper,
        IHubContext<EventHub> hubContext)
    {
        _eventRepository = eventRepository;
        _userRepository = userRepository;
        _context = context;
        _mapper = mapper;
        _hubContext = hubContext;
    }

    public async Task<IEnumerable<EventDTO>> GetAllEventsAsync(CancellationToken cancellationToken = default)
    {
        var events = await _eventRepository.BuildSearchQuery()
            .OrderByDescending(e => e.StartTime)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<EventDTO>>(events);
    }

    public async Task<EventDTO?> GetEventByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ev = await _eventRepository.BuildSearchQuery()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        return ev == null ? null : _mapper.Map<EventDTO>(ev);
    }

    public async Task<Event?> GetEventEntityByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _eventRepository.BuildSearchQuery()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<EventDTO> CreateEventAsync(EventCreateDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto.StartTime >= dto.EndTime)
            throw new ArgumentException("Ngày kết thúc phải sau ngày bắt đầu.");

        var newEvent = _mapper.Map<Event>(dto);
        newEvent.Id = Guid.NewGuid();
        newEvent.CreatedAt = DateTime.UtcNow;
        newEvent.RegisteredCount = 0;

        if (dto.CategoryIds != null && dto.CategoryIds.Any())
        {
            newEvent.EventCategories = dto.CategoryIds.Select(id => new EventCategory { CategoryId = id, EventId = newEvent.Id }).ToList();
        }

        if (dto.TagIds != null && dto.TagIds.Any())
        {
            newEvent.EventTags = dto.TagIds.Select(id => new EventTag { TagId = id, EventId = newEvent.Id }).ToList();
        }

        await _eventRepository.AddAsync(newEvent, cancellationToken);
        
        // Tự động tạo bản ghi EventReminder đi kèm sự kiện mới
        var reminder = new EventReminder
        {
            EventId = newEvent.Id,
            ScheduledTime = newEvent.StartTime.AddDays(-1),
            IsEmailSent = false
        };
        await _context.EventReminders.AddAsync(reminder, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        // Reload để có navigation properties
        var created = await _eventRepository.BuildSearchQuery()
            .FirstOrDefaultAsync(e => e.Id == newEvent.Id, cancellationToken);

        return _mapper.Map<EventDTO>(created!);
    }

    public async Task UpdateEventAsync(EventUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        if (dto.StartTime >= dto.EndTime)
            throw new ArgumentException("Ngày kết thúc phải sau ngày bắt đầu.");

        var ev = await _eventRepository.Query()
            .Include(e => e.EventCategories)
            .Include(e => e.EventTags)
            .Include(e => e.EventReminder)
            .FirstOrDefaultAsync(e => e.Id == dto.Id, cancellationToken);

        if (ev == null)
            throw new KeyNotFoundException("Sự kiện không tồn tại.");

        var oldStartTime = ev.StartTime;

        if (ev.VenueId != dto.VenueId)
        {
            var newVenue = await _context.Venues.FirstOrDefaultAsync(v => v.Id == dto.VenueId, cancellationToken);
            if (newVenue != null && ev.RegisteredCount > newVenue.MaxCapacity)
            {
                throw new InvalidOperationException($"Sự kiện đã có {ev.RegisteredCount} lượt đăng ký, không thể chuyển sang địa điểm có sức chứa {newVenue.MaxCapacity}.");
            }
        }

        _mapper.Map(dto, ev);

        ev.EventCategories.Clear();
        if (dto.CategoryIds != null)
        {
            foreach (var id in dto.CategoryIds)
                ev.EventCategories.Add(new EventCategory { CategoryId = id, EventId = ev.Id });
        }

        ev.EventTags.Clear();
        if (dto.TagIds != null)
        {
            foreach (var id in dto.TagIds)
                ev.EventTags.Add(new EventTag { TagId = id, EventId = ev.Id });
        }

        // Nếu cập nhật thời gian bắt đầu của sự kiện, cập nhật lại lịch nhắc nhở EventReminder tương ứng
        if (ev.StartTime != oldStartTime)
        {
            if (ev.EventReminder != null)
            {
                ev.EventReminder.ScheduledTime = ev.StartTime.AddDays(-1);
                ev.EventReminder.IsEmailSent = false;
                ev.EventReminder.SentAt = null;
            }
            else
            {
                var newReminder = new EventReminder
                {
                    EventId = ev.Id,
                    ScheduledTime = ev.StartTime.AddDays(-1),
                    IsEmailSent = false
                };
                await _context.EventReminders.AddAsync(newReminder, cancellationToken);
            }
        }

        _eventRepository.Update(ev);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteEventAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ev = await _eventRepository.Query()
            .Include(e => e.Bookings)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (ev == null)
            throw new KeyNotFoundException("Sự kiện không tồn tại.");

        if (ev.Bookings != null && ev.Bookings.Any())
            throw new InvalidOperationException("Không thể xóa sự kiện này vì đã có sinh viên Đăng ký.");

        try
        {
            _eventRepository.Remove(ev);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Không thể xóa sự kiện này vì đã có sinh viên Đăng ký hoặc ràng buộc dữ liệu.");
        }
    }

    public async Task<IEnumerable<User>> GetOrganizersAsync(CancellationToken cancellationToken = default)
    {
        return await _userRepository.Query()
            .Where(u => u.Role == "Organizer")
            .OrderBy(u => u.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task ChangeEventStatusAsync(Guid id, string newStatus, CancellationToken cancellationToken = default)
    {
        var ev = await _eventRepository.Query()
            .Include(e => e.Venue)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (ev == null)
            throw new KeyNotFoundException("Sự kiện không tồn tại.");

        ev.Status = newStatus;
        
        _eventRepository.Update(ev);
        await _context.SaveChangesAsync(cancellationToken);

        // Phát tín hiệu SignalR khi duyệt (Publish) sự kiện để đồng bộ Dashboard thời gian thực
        if (newStatus == "Published")
        {
            var maxCapacity = ev.Venue?.MaxCapacity ?? 0;
            await _hubContext.Clients.All.SendAsync("ReceiveEventPublished", ev.Id, ev.Title, maxCapacity, cancellationToken);
        }
    }
}
