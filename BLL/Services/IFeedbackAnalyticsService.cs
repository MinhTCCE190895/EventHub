using BLL.DTOs;

namespace BLL.Services;

public interface IFeedbackAnalyticsService
{
    /// <summary>
    /// Kiểm tra xem sinh viên có thể đánh giá sự kiện hay không (chỉ cho phép khi có vé "Confirmed").
    /// </summary>
    Task<bool> CanSubmitFeedbackAsync(Guid eventId, Guid studentId);

    /// <summary>
    /// Lưu đánh giá mới của sinh viên.
    /// </summary>
    Task SubmitFeedbackAsync(FeedbackSubmissionDto dto);

    /// <summary>
    /// Thống kê điểm số phản hồi đa tiêu chí của tất cả sự kiện bằng PLINQ (tối ưu hóa song song trên CPU).
    /// </summary>
    Task<IEnumerable<EventFeedbackMetricsDto>> GetFeedbackMetricsAsync(CancellationToken cancellationToken = default);
}
