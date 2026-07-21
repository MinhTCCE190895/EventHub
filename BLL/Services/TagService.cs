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

public class TagService : ITagService
{
    private readonly IRepository<Tag> _tagRepository;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public TagService(IRepository<Tag> tagRepository, AppDbContext context, IMapper mapper)
    {
        _tagRepository = tagRepository;
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<TagDTO>> GetAllTagsAsync(CancellationToken cancellationToken = default)
    {
        var tags = await _tagRepository.Query()
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
            
        return _mapper.Map<IEnumerable<TagDTO>>(tags);
    }

    public async Task<TagDTO?> GetTagByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var tag = await _tagRepository.Query()
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        
        if (tag == null)
        {
            return null;
        }

        return _mapper.Map<TagDTO>(tag);
    }

    public async Task<TagDTO> CreateTagAsync(TagCreateDTO dto, CancellationToken cancellationToken = default)
    {
        dto.Name = dto.Name?.Trim() ?? string.Empty;
        var exists = await _tagRepository.Query().AnyAsync(t => t.Name.ToLower() == dto.Name.ToLower(), cancellationToken);
        if (exists) throw new InvalidOperationException("Tên thẻ đã tồn tại.");

        var newTag = _mapper.Map<Tag>(dto);

        await _tagRepository.AddAsync(newTag, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TagDTO>(newTag);
    }

    public async Task UpdateTagAsync(TagUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        dto.Name = dto.Name?.Trim() ?? string.Empty;
        var exists = await _tagRepository.Query().AnyAsync(t => t.Name.ToLower() == dto.Name.ToLower() && t.Id != dto.Id, cancellationToken);
        if (exists) throw new InvalidOperationException("Tên thẻ đã tồn tại.");

        var tag = await _tagRepository.Query()
            .FirstOrDefaultAsync(t => t.Id == dto.Id, cancellationToken);

        if (tag == null)
        {
            throw new KeyNotFoundException("Tag not found.");
        }

        _mapper.Map(dto, tag);
        _tagRepository.Update(tag);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteTagAsync(int id, CancellationToken cancellationToken = default)
    {
        var tag = await _tagRepository.Query()
            .Include(t => t.EventTags)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (tag == null)
        {
            throw new KeyNotFoundException("Tag not found.");
        }

        if (tag.EventTags != null && tag.EventTags.Any())
        {
            throw new InvalidOperationException("Không thể xóa thẻ này vì đang có Sự kiện gắn với thẻ này.");
        }

        try
        {
            _tagRepository.Remove(tag);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Không thể xóa thẻ này vì đang có Sự kiện gắn với thẻ này.");
        }
    }
}