using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusinessObjects.DTOs;

namespace BLL.Interfaces;

public interface ICategoryService
{
    Task<IEnumerable<CategoryDTO>> GetAllCategoriesAsync(CancellationToken cancellationToken = default);
    Task<CategoryDTO?> GetCategoryByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<CategoryDTO> CreateCategoryAsync(CategoryCreateDTO dto, CancellationToken cancellationToken = default);
    Task UpdateCategoryAsync(CategoryUpdateDTO dto, CancellationToken cancellationToken = default);
    Task DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);
}
