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

public class CategoryService : ICategoryService
{
    private readonly IRepository<Category> _categoryRepository;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public CategoryService(IRepository<Category> categoryRepository, AppDbContext context, IMapper mapper)
    {
        _categoryRepository = categoryRepository;
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _categoryRepository.Query()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
            
        return _mapper.Map<IEnumerable<CategoryDTO>>(categories);
    }

    public async Task<CategoryDTO?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.Query()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        
        if (category == null)
        {
            return null;
        }

        return _mapper.Map<CategoryDTO>(category);
    }

    public async Task<CategoryDTO> CreateCategoryAsync(CategoryCreateDTO dto, CancellationToken cancellationToken = default)
    {
        dto.Name = dto.Name?.Trim() ?? string.Empty;
        var exists = await _categoryRepository.Query().AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower(), cancellationToken);
        if (exists) throw new InvalidOperationException("Tên danh mục đã tồn tại.");

        var newCategory = _mapper.Map<Category>(dto);

        await _categoryRepository.AddAsync(newCategory, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<CategoryDTO>(newCategory);
    }

    public async Task UpdateCategoryAsync(CategoryUpdateDTO dto, CancellationToken cancellationToken = default)
    {
        dto.Name = dto.Name?.Trim() ?? string.Empty;
        var exists = await _categoryRepository.Query().AnyAsync(c => c.Name.ToLower() == dto.Name.ToLower() && c.Id != dto.Id, cancellationToken);
        if (exists) throw new InvalidOperationException("Tên danh mục đã tồn tại.");

        var category = await _categoryRepository.Query()
            .FirstOrDefaultAsync(c => c.Id == dto.Id, cancellationToken);

        if (category == null)
        {
            throw new KeyNotFoundException("Category not found.");
        }

        _mapper.Map(dto, category);
        _categoryRepository.Update(category);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _categoryRepository.Query()
            .Include(c => c.EventCategories)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

        if (category == null)
        {
            throw new KeyNotFoundException("Category not found.");
        }

        if (category.EventCategories != null && category.EventCategories.Any())
        {
            throw new InvalidOperationException("Không thể xóa danh mục này vì đang có Sự kiện gắn với danh mục này.");
        }

        try
        {
            _categoryRepository.Remove(category);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Không thể xóa danh mục này vì đang có Sự kiện gắn với danh mục này.");
        }
    }
}
