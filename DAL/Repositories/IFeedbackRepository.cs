using DAL.Entities;

namespace DAL.Repositories;

public interface IFeedbackRepository : IRepository<Feedback>
{
    /// <summary>
    /// Lấy danh sách đánh giá của sự kiện cụ thể kèm theo chi tiết điểm tiêu chí.
    /// </summary>
    Task<IEnumerable<Feedback>> GetFeedbacksByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy toàn bộ đánh giá hệ thống cùng chi tiết điểm tiêu chí và thông tin sự kiện liên quan.
    /// </summary>
    Task<IEnumerable<Feedback>> GetAllFeedbacksWithDetailsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lấy danh sách đánh giá của sinh viên cụ thể kèm theo chi tiết điểm tiêu chí và thông tin sự kiện.
    /// </summary>
    Task<IEnumerable<Feedback>> GetFeedbacksByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default);
}
