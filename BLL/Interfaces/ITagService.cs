using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusinessObjects.DTOs;

namespace BLL.Interfaces;

public interface ITagService
{
    Task<IEnumerable<TagDTO>> GetAllTagsAsync(CancellationToken cancellationToken = default);
    Task<TagDTO?> GetTagByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TagDTO> CreateTagAsync(TagCreateDTO dto, CancellationToken cancellationToken = default);
    Task UpdateTagAsync(TagUpdateDTO dto, CancellationToken cancellationToken = default);
    Task DeleteTagAsync(int id, CancellationToken cancellationToken = default);
}
