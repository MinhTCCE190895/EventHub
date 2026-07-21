using BLL.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BLL.DTOs;
using DAL.Data;
using DAL.Entities;
using DAL.Repositories;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class VenueService : IVenueService
{
    private readonly IRepository<Venue> _venueRepository;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public VenueService(IRepository<Venue> venueRepository, AppDbContext context, IMapper mapper)
    {
        _venueRepository = venueRepository;
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<VenueDTO>> GetAllVenuesAsync(CancellationToken cancellationToken = default)
    {
        var venues = await _venueRepository.Query()
            .OrderBy(v => v.Name)
            .ToListAsync(cancellationToken);
            
        return _mapper.Map<IEnumerable<VenueDTO>>(venues);
    }

    public async Task<VenueDTO?> GetVenueByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var venue = await _venueRepository.Query()
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        
        if (venue == null)
        {
            return null;
        }

        return _mapper.Map<VenueDTO>(venue);
    }

    public async Task<VenueDTO> CreateVenueAsync(VenueCreateDTO dto, CancellationToken cancellationToken = default)
    {
        var newVenue = _mapper.Map<Venue>(dto);

        await _venueRepository.AddAsync(newVenue, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<VenueDTO>(newVenue);
    }

    public async Task UpdateVenueAsync(VenueUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        var venue = await _venueRepository.Query()
            .Include(v => v.Events)
            .FirstOrDefaultAsync(v => v.Id == dto.Id, cancellationToken);

        if (venue == null)
        {
            throw new KeyNotFoundException("Venue not found.");
        }

        var maxRegistered = venue.Events.Any() ? venue.Events.Max(e => e.RegisteredCount) : 0;
        if (dto.MaxCapacity < maxRegistered)
        {
            throw new InvalidOperationException($"Không thể giảm sức chứa xuống {dto.MaxCapacity} vì đang có sự kiện có {maxRegistered} lượt đăng ký.");
        }

        _mapper.Map(dto, venue);
        _venueRepository.Update(venue);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteVenueAsync(int id, CancellationToken cancellationToken = default)
    {
        var venue = await _venueRepository.Query()
            .Include(v => v.Events)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);

        if (venue == null)
        {
            throw new KeyNotFoundException("Venue not found.");
        }

        if (venue.Events != null && venue.Events.Any())
        {
            throw new InvalidOperationException("Không thể xóa địa điểm này vì đang có sự kiện được tổ chức tại đây");
        }

        try
        {
            _venueRepository.Remove(venue);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Không thể xóa địa điểm này vì đang có sự kiện được tổ chức tại đây");
        }
    }
}