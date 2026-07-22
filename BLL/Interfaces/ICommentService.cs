using BLL.DTOs;

namespace BLL.Interfaces;

public interface ICommentService
{
    Task<IEnumerable<CommentDTO>> GetCommentsForEventAsync(Guid eventId, CancellationToken cancellationToken = default);
    Task<CommentDTO> AddCommentAsync(Guid eventId, Guid userId, string content, Guid? parentCommentId = null, CancellationToken cancellationToken = default);
    Task<bool> HideCommentAsync(Guid commentId, Guid adminUserId, CancellationToken cancellationToken = default);
    Task<bool> DeleteCommentAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default);
}
