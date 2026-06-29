using BusinessObjects.DTOs;

namespace BLL.Interfaces;

public interface IFeedbackAnalyticsService
{
    /// Kiểm tra xem sinh viên có thể đánh giá sự kiện hay không (chỉ cho phép khi có vé "Confirmed").
    Task<bool> CanSubmitFeedbackAsync(Guid eventId, Guid studentId);

    /// Lưu đánh giá mới của sinh viên.
    Task SubmitFeedbackAsync(FeedbackSubmissionDto dto);

    /// Thống kê điểm số phản hồi đa tiêu chí của tất cả sự kiện bằng PLINQ (tối ưu hóa song song trên CPU).
    Task<IEnumerable<EventFeedbackMetricsDto>> GetFeedbackMetricsAsync(CancellationToken cancellationToken = default);

    /// Lấy danh sách phản hồi chi tiết của một sự kiện (bao gồm bình luận của sinh viên).
    Task<IEnumerable<EventFeedbackDetailDto>> GetFeedbacksByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);
    /// Lấy danh sách phản hồi mà sinh viên đã gửi.
    Task<IEnumerable<StudentFeedbackDto>> GetFeedbacksByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
}
