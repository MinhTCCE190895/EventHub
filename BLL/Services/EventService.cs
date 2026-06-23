using AutoMapper;
using BusinessObjects.DTOs;
using DAL.Data;
using DAL.Entities;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BLL.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IRepository<User> _userRepository;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public EventService(
        IEventRepository eventRepository,
        IRepository<User> userRepository,
        AppDbContext context,
        IMapper mapper)
    {
        _eventRepository = eventRepository;
        _userRepository = userRepository;
        _context = context;
        _mapper = mapper;
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

    public async Task<EventDTO> CreateEventAsync(EventCreateDTO dto, CancellationToken cancellationToken = default)
    {
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
        await _context.SaveChangesAsync(cancellationToken);

        // Reload để có navigation properties
        var created = await _eventRepository.BuildSearchQuery()
            .FirstOrDefaultAsync(e => e.Id == newEvent.Id, cancellationToken);

        return _mapper.Map<EventDTO>(created!);
    }

    public async Task UpdateEventAsync(EventUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        var ev = await _eventRepository.Query()
            .Include(e => e.EventCategories)
            .Include(e => e.EventTags)
            .FirstOrDefaultAsync(e => e.Id == dto.Id, cancellationToken);

        if (ev == null)
            throw new KeyNotFoundException("Sự kiện không tồn tại.");

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

        _eventRepository.Update(ev);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteEventAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var ev = await _eventRepository.Query()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (ev == null)
            throw new KeyNotFoundException("Sự kiện không tồn tại.");

        _eventRepository.Remove(ev);
        await _context.SaveChangesAsync(cancellationToken);
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
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        if (ev == null)
            throw new KeyNotFoundException("Sự kiện không tồn tại.");

        ev.Status = newStatus;
        
        _eventRepository.Update(ev);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
