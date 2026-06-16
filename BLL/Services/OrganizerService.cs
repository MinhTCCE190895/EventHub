using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using BusinessObjects.DTOs;
using DAL.Data;
using DAL.Entities;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class OrganizerService : IOrganizerService
{
    private readonly IRepository<User> _userRepository;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public OrganizerService(IRepository<User> userRepository, AppDbContext context, IMapper mapper)
    {
        _userRepository = userRepository;
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<OrganizerDTO>> GetAllOrganizersAsync(CancellationToken cancellationToken = default)
    {
        var organizers = await _userRepository.Query()
            .Where(u => u.Role == "Organizer")
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);
            
        return _mapper.Map<IEnumerable<OrganizerDTO>>(organizers);
    }

    public async Task<OrganizerDTO?> GetOrganizerByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var organizer = await _userRepository.Query()
            .FirstOrDefaultAsync(u => u.Id == id && u.Role == "Organizer", cancellationToken);
        
        if (organizer == null)
        {
            return null;
        }

        return _mapper.Map<OrganizerDTO>(organizer);
    }

    public async Task<OrganizerDTO> CreateOrganizerAsync(OrganizerCreateDTO dto, CancellationToken cancellationToken = default)
    {
        var newOrganizer = _mapper.Map<User>(dto);
        newOrganizer.Id = Guid.NewGuid();
        newOrganizer.Role = "Organizer";
        newOrganizer.IsActive = true;
        newOrganizer.CreatedAt = DateTime.UtcNow;

        await _userRepository.AddAsync(newOrganizer, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<OrganizerDTO>(newOrganizer);
    }

    public async Task UpdateOrganizerAsync(OrganizerUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        var organizer = await _userRepository.Query()
            .FirstOrDefaultAsync(u => u.Id == dto.Id && u.Role == "Organizer", cancellationToken);

        if (organizer == null)
        {
            throw new KeyNotFoundException("Organizer not found.");
        }

        _mapper.Map(dto, organizer);
        _userRepository.Update(organizer);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteOrganizerAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var organizer = await _userRepository.Query()
            .FirstOrDefaultAsync(u => u.Id == id && u.Role == "Organizer", cancellationToken);

        if (organizer == null)
        {
            throw new KeyNotFoundException("Organizer not found.");
        }

        _userRepository.Remove(organizer);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
