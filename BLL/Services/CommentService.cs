using BLL.Interfaces;
using AutoMapper;
using BLL.DTOs;
using DAL.Entities;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using DAL.Data;

namespace BLL.Services;

public class CommentService : ICommentService
{
    private readonly IRepository<EventComment> _commentRepository;
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;

    public CommentService(
        IRepository<EventComment> commentRepository,
        AppDbContext context,
        IMapper mapper)
    {
        _commentRepository = commentRepository;
        _context = context;
        _mapper = mapper;
    }

    public async Task<IEnumerable<CommentDTO>> GetCommentsForEventAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var comments = await _commentRepository.Query()
            .Include(c => c.User)
            .Include(c => c.Replies)
                .ThenInclude(r => r.User)
            .Where(c => c.EventId == eventId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        var topLevel = comments.Where(c => c.ParentCommentId == null).ToList();
        var dtos = _mapper.Map<List<CommentDTO>>(topLevel);

        void MapRepliesRecursively(List<EventComment> entities, List<CommentDTO> dtoTargetList)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                var childEntities = comments.Where(c => c.ParentCommentId == entities[i].Id).OrderBy(c => c.CreatedAt).ToList();
                dtoTargetList[i].Replies = _mapper.Map<List<CommentDTO>>(childEntities);
                if (childEntities.Any())
                {
                    MapRepliesRecursively(childEntities, dtoTargetList[i].Replies);
                }
            }
        }

        MapRepliesRecursively(topLevel, dtos);
        return dtos;
    }

    public async Task<CommentDTO> AddCommentAsync(Guid eventId, Guid userId, string content, Guid? parentCommentId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Nội dung bình luận không được để trống.");

        if (parentCommentId.HasValue)
        {
            var parentExists = await _commentRepository.Query().AnyAsync(c => c.Id == parentCommentId.Value, cancellationToken);
            if (!parentExists)
            {
                throw new KeyNotFoundException("Bình luận cha không tồn tại.");
            }
        }

        var comment = new EventComment
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            ParentCommentId = parentCommentId,
            Content = content.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _commentRepository.AddAsync(comment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        // Load reference to get User Name
        var inserted = await _commentRepository.Query()
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == comment.Id, cancellationToken);

        return _mapper.Map<CommentDTO>(inserted!);
    }

    public async Task<bool> HideCommentAsync(Guid commentId, Guid adminUserId, CancellationToken cancellationToken = default)
    {
        var admin = await _context.Users.FirstOrDefaultAsync(u => u.Id == adminUserId, cancellationToken);
        if (admin == null || admin.Role != "Admin")
        {
            throw new UnauthorizedAccessException("Chỉ có Quản trị viên (Admin) mới có quyền ẩn bình luận.");
        }

        var comment = await _commentRepository.Query()
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);

        if (comment == null)
        {
            throw new KeyNotFoundException("Bình luận không tồn tại.");
        }

        comment.IsHidden = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteCommentAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Không xác định được danh tính người dùng.");
        }

        var comment = await _commentRepository.Query()
            .Include(c => c.Replies)
            .FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);

        if (comment == null)
        {
            throw new KeyNotFoundException("Bình luận không tồn tại.");
        }

        if (comment.UserId != userId && user.Role != "Admin")
        {
            throw new UnauthorizedAccessException("Bạn chỉ có quyền xóa bình luận của chính mình.");
        }

        // Xóa tất cả bình luận con liên quan
        if (comment.Replies != null && comment.Replies.Any())
        {
            _context.Set<EventComment>().RemoveRange(comment.Replies);
        }

        _context.Set<EventComment>().Remove(comment);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
