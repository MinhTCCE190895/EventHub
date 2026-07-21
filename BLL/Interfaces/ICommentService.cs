using BLL.DTOs;

namespace BLL.Interfaces;

public interface ICommentService
{
    Task<IEnumerable<CommentDTO>> GetCommentsForEventAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<CommentDTO> AddCommentAsync(Guid eventId, Guid userId, string content, CancellationToken cancellationToken = default);
    Task<bool> HideCommentAsync(Guid commentId, Guid adminUserId, CancellationToken cancellationToken = default);
}
