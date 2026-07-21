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
            .Where(c => c.EventId == eventId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return _mapper.Map<IEnumerable<CommentDTO>>(comments);
    }

    public async Task<CommentDTO> AddCommentAsync(Guid eventId, Guid userId, string content, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Nội dung bình luận không được để trống.");

        var comment = new EventComment
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
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

        if (comment.User != null && comment.User.Role == "Admin")
        {
            throw new InvalidOperationException("Không thể ẩn bình luận của Quản trị viên.");
        }

        comment.IsHidden = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
