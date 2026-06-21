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

    /// <summary>
    /// Lấy danh sách phản hồi chi tiết của một sự kiện (bao gồm bình luận của sinh viên).
    /// </summary>
    Task<IEnumerable<EventFeedbackDetailDto>> GetFeedbacksByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách phản hồi mà sinh viên đã gửi.
    /// </summary>
    Task<IEnumerable<StudentFeedbackDto>> GetFeedbacksByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
}
