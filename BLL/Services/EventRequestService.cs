using AutoMapper;
using BLL.Interfaces;
using BLL.DTOs;
using DAL.Data;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class EventRequestService : IEventRequestService
{
    private readonly IRepository<EventRequest> _requestRepository;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public EventRequestService(
        IRepository<EventRequest> requestRepository,
        AppDbContext context,
        IMapper mapper)
    {
        _requestRepository = requestRepository;
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<EventRequestDTO>> GetAllRequestsAsync(string? statusFilter = null, CancellationToken cancellationToken = default)
    {
        var query = _requestRepository.Query()
            .Include(r => r.Student)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            query = query.Where(r => r.Status.ToLower() == statusFilter.Trim().ToLower());
        }

        var requests = await query
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<EventRequestDTO>>(requests);
    }

    public async Task<IEnumerable<EventRequestDTO>> GetRequestsByStudentIdAsync(Guid studentId, string? statusFilter = null, CancellationToken cancellationToken = default)
    {
        var query = _requestRepository.Query()
            .Include(r => r.Student)
            .Where(r => r.StudentId == studentId)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(statusFilter))
        {
            query = query.Where(r => r.Status.ToLower() == statusFilter.Trim().ToLower());
        }

        var requests = await query
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<EventRequestDTO>>(requests);
    }

    public async Task<EventRequestDTO?> GetRequestByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var request = await _requestRepository.Query()
            .Include(r => r.Student)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        return request == null ? null : _mapper.Map<EventRequestDTO>(request);
    }

    public async Task<EventRequestDTO> CreateRequestAsync(Guid studentId, EventRequestCreateDTO dto, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<EventRequest>(dto);
        entity.StudentId = studentId;
        entity.Status = "Pending";
        entity.SubmittedAt = DateTime.UtcNow;

        await _requestRepository.AddAsync(entity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var created = await _requestRepository.Query()
            .Include(r => r.Student)
            .FirstOrDefaultAsync(r => r.Id == entity.Id, cancellationToken);

        return _mapper.Map<EventRequestDTO>(created!);
    }

    public async Task ProcessRequestAsync(EventRequestProcessDTO dto, CancellationToken cancellationToken = default)
    {
        var entity = await _requestRepository.Query()
            .FirstOrDefaultAsync(r => r.Id == dto.Id, cancellationToken);

        if (entity == null)
        {
            throw new KeyNotFoundException("Yêu cầu ý tưởng không tồn tại.");
        }

        entity.Status = dto.Status;
        entity.ResponseMessage = dto.ResponseMessage;

        _requestRepository.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
