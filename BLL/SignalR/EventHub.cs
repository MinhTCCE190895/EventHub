using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace BLL.SignalR;

public class EventHub : Hub
{
    // Dictionary tĩnh để lưu vết thời gian gửi tin nhắn cuối cùng của mỗi ConnectionId nhằm chống spam comment (Rate Limit)
    private static readonly ConcurrentDictionary<string, DateTime> _lastCommentTimes = new();

    // Client sẽ lắng nghe sự kiện "ReceiveTicketUpdate" để cập nhật số lượng chỗ trống theo thời gian thực

    // Phương thức gửi bình luận thời gian thực có cơ chế chặn spam (Rate Limiting 2 giây)
    public async Task SendComment(Guid eventId, string userName, string commentText)
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

        // Broadcast bình luận mới đến tất cả các client đang kết nối
        await Clients.All.SendAsync("ReceiveComment", eventId, userName, commentText, now.ToLocalTime().ToString("HH:mm:ss"));
    }

    // Tự động dọn dẹp bộ nhớ cache connectionId khi client ngắt kết nối
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _lastCommentTimes.TryRemove(Context.ConnectionId, out _);
        return base.OnDisconnectedAsync(exception);
    }
}
