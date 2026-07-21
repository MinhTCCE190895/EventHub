using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using BLL.Interfaces;

namespace BLL.SignalR;

public class EventHub : Hub
{
    private readonly ICommentService _commentService;

    // Dictionary tĩnh để lưu vết thời gian gửi tin nhắn cuối cùng của mỗi ConnectionId nhằm chống spam comment (Rate Limit)
    private static readonly ConcurrentDictionary<string, DateTime> _lastCommentTimes = new();

    public EventHub(ICommentService commentService)
    {
        _commentService = commentService;
    }

    // Phương thức gửi bình luận thời gian thực có cơ chế chặn spam (Rate Limiting 2 giây)
    public async Task SendComment(Guid eventId, Guid userId, string commentText)
    {
        var connectionId = Context.ConnectionId;
        var now = DateTime.UtcNow;

        // Kiểm tra xem connectionId này đã gửi comment trước đó chưa
        if (_lastCommentTimes.TryGetValue(connectionId, out var lastTime))
        {
            // Nếu khoảng cách giữa 2 lần gửi nhỏ hơn 2 giây thì chặn lại và báo lỗi spam
            if ((now - lastTime).TotalSeconds < 2)
            {
                throw new HubException("Bạn đang gửi bình luận quá nhanh. Vui lòng chờ 2 giây để gửi bình luận tiếp theo.");
            }
        }

        // Cập nhật mốc thời gian gửi comment mới nhất
        _lastCommentTimes[connectionId] = now;

        // Lưu comment vào database qua Service
        var comment = await _commentService.AddCommentAsync(eventId, userId, commentText);

        // Broadcast bình luận mới đến tất cả các client đang kết nối
        await Clients.All.SendAsync("ReceiveComment", eventId, comment.UserFullName, comment.Content, comment.CreatedAt.ToLocalTime().ToString("HH:mm:ss"), comment.UserRole);
    }

    // Phương thức ẩn bình luận dành cho Admin
    public async Task HideComment(Guid commentId, Guid adminUserId)
    {
        await _commentService.HideCommentAsync(commentId, adminUserId);
        await Clients.All.SendAsync("ReceiveCommentHidden", commentId);
    }

    // Tự động dọn dẹp bộ nhớ cache connectionId khi client ngắt kết nối
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _lastCommentTimes.TryRemove(Context.ConnectionId, out _);
        return base.OnDisconnectedAsync(exception);
    }
}
