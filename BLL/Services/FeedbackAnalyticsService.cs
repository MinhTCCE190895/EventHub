using BLL.DTOs;
using DAL.Data;
using DAL.Entities;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace BLL.Services;

public class FeedbackAnalyticsService : IFeedbackAnalyticsService
{
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly AppDbContext _context;
    private readonly ILogger<FeedbackAnalyticsService> _logger;

    public FeedbackAnalyticsService(
        IFeedbackRepository feedbackRepository,
        IBookingRepository bookingRepository,
        AppDbContext context,
        ILogger<FeedbackAnalyticsService> logger)
    {
        _feedbackRepository = feedbackRepository;
        _bookingRepository = bookingRepository;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// LUỒNG KIỂM TRA QUYỀN ĐÁNH GIÁ:
    /// 1. Tìm Booking ứng với EventId và StudentId.
    /// 2. Vé phải ở trạng thái "Confirmed".
    /// 3. Người dùng chưa từng gửi Feedback cho Booking này.
    /// </summary>
    public async Task<bool> CanSubmitFeedbackAsync(Guid eventId, Guid studentId)
    {
        var booking = await _bookingRepository.Query()
            .Include(b => b.Feedback)
            .FirstOrDefaultAsync(b => b.EventId == eventId && b.StudentId == studentId && b.Status == "Confirmed");

        if (booking == null) return false;
        
        // Nếu đã có đánh giá rồi thì không cho phép đánh giá tiếp
        return booking.Feedback == null;
    }

    /// <summary>
    /// LUỒNG LƯU ĐÁNH GIÁ MỚI:
    /// 1. Tạo bản ghi Feedback.
    /// 2. Tạo danh sách FeedbackDetail tương ứng với các tiêu chí và điểm số gửi lên.
    /// 3. Lưu dữ liệu xuống database.
    /// </summary>
    public async Task SubmitFeedbackAsync(FeedbackSubmissionDto dto)
    {
        // Kiểm tra xem booking có tồn tại không
        var bookingExists = await _bookingRepository.ExistsAsync(b => b.Id == dto.BookingId);
        if (!bookingExists)
        {
            throw new KeyNotFoundException("Không tìm thấy thông tin đặt vé hợp lệ.");
        }

        // Bắt đầu lưu Feedback
        var feedback = new Feedback
        {
            Id = Guid.NewGuid(),
            BookingId = dto.BookingId,
            GeneralComment = dto.GeneralComment,
            SubmittedAt = DateTime.UtcNow
        };

        foreach (var detailDto in dto.Details)
        {
            feedback.FeedbackDetails.Add(new FeedbackDetail
            {
                Criteria = detailDto.Criteria,
                Score = detailDto.Score
            });
        }

        await _feedbackRepository.AddAsync(feedback);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// LUỒNG THỐNG KÊ PHÂN TÍCH PHẢN HỒI (PLINQ Core Engine):
    /// 1. Lấy toàn bộ danh sách feedback từ DB kèm FeedbackDetails, Booking, Event.
    /// 2. Sử dụng LINQ gom nhóm (GroupBy) theo EventId để phân loại đánh giá cho từng sự kiện.
    /// 3. Ứng với mỗi sự kiện, sử dụng PLINQ (.AsParallel()) để xử lý song song các tác vụ tính toán điểm trên các CPU core:
    ///    - Lọc và gom nhóm các chi tiết tiêu chí (Criteria).
    ///    - Tính toán điểm trung bình song song cho từng tiêu chí (Diễn giả, Hậu cần, Nội dung, Tổ chức).
    ///    - Tính toán điểm trung bình tổng quan (Overall Average Score).
    /// 4. Trả về kết quả tổng hợp.
    /// </summary>
    public async Task<IEnumerable<EventFeedbackMetricsDto>> GetFeedbackMetricsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(">>> [PLINQ Engine] Khởi chạy tính toán song song thống kê Feedback...");

        // Bước 1: Lấy toàn bộ dữ liệu phản hồi
        var feedbacks = await _feedbackRepository.GetAllFeedbacksWithDetailsAsync(cancellationToken);
        var feedbacksList = feedbacks.ToList();

        if (!feedbacksList.Any())
        {
            return Enumerable.Empty<EventFeedbackMetricsDto>();
        }

        // Bước 2: Group feedbacks by Event
        var groupedByEvent = feedbacksList
            .Where(f => f.Booking != null && f.Booking.Event != null)
            .GroupBy(f => f.Booking.EventId)
            .ToList();

        // Bước 3: Áp dụng PLINQ song song để xử lý đồng thời tính toán điểm cho nhiều sự kiện
        // PLINQ sẽ tự động chia nhỏ mảng công việc và tận dụng các lõi CPU chạy song song.
        var metrics = groupedByEvent
            .AsParallel() // Kích hoạt xử lý song song PLINQ
            .WithCancellation(cancellationToken)
            .Select(group =>
            {
                var eventId = group.Key;
                var firstFeedback = group.First();
                var eventTitle = firstFeedback.Booking.Event.Title;
                var totalFeedbacks = group.Count();

                // Lấy tất cả FeedbackDetails thuộc nhóm sự kiện này
                var allDetails = group.SelectMany(f => f.FeedbackDetails).ToList();

                // Dùng PLINQ để tính toán điểm trung bình cho từng tiêu chí (Criteria) song song
                var criteriaAverages = allDetails
                    .AsParallel() // Chạy PLINQ song song
                    .GroupBy(d => d.Criteria)
                    .ToDictionary(
                        g => g.Key,
                        g => Math.Round(g.Average(d => d.Score), 1)
                    );

                // Tính toán điểm trung bình tổng quan cho toàn bộ sự kiện song song
                var overallAverage = allDetails.Any()
                    ? Math.Round(allDetails.AsParallel().Average(d => d.Score), 1)
                    : 0.0;

                return new EventFeedbackMetricsDto
                {
                    EventId = eventId,
                    EventTitle = eventTitle,
                    TotalFeedbacks = totalFeedbacks,
                    OverallAverageScore = overallAverage,
                    CriteriaAverages = criteriaAverages
                };
            })
            .ToList();

        _logger.LogInformation(">>> [PLINQ Engine] Hoàn tất tính toán thống kê phản hồi cho {Count} sự kiện.", metrics.Count);
        return metrics;
    }

    public async Task<IEnumerable<EventFeedbackDetailDto>> GetFeedbacksByEventIdAsync(Guid eventId, CancellationToken cancellationToken = default)
    {
        var feedbacks = await _feedbackRepository.GetFeedbacksByEventIdAsync(eventId, cancellationToken);
        var feedbacksList = feedbacks.ToList();

        if (!feedbacksList.Any())
        {
            return Enumerable.Empty<EventFeedbackDetailDto>();
        }

        return feedbacksList.AsParallel()
            .Select(f => new EventFeedbackDetailDto
            {
                FeedbackId = f.Id,
                StudentName = f.Booking?.Student?.FullName ?? "N/A",
                StudentEmail = f.Booking?.Student?.Email ?? "N/A",
                GeneralComment = f.GeneralComment,
                SubmittedAt = f.SubmittedAt,
                CriteriaScores = f.FeedbackDetails.ToDictionary(fd => fd.Criteria, fd => fd.Score),
                AverageScore = f.FeedbackDetails.Any() ? Math.Round(f.FeedbackDetails.Average(fd => fd.Score), 1) : 0.0
            })
            .ToList();
    }

    public async Task<IEnumerable<StudentFeedbackDto>> GetFeedbacksByStudentIdAsync(Guid studentId, CancellationToken cancellationToken = default)
    {
        var feedbacks = await _feedbackRepository.GetFeedbacksByStudentIdAsync(studentId, cancellationToken);
        var feedbacksList = feedbacks.ToList();

        if (!feedbacksList.Any())
        {
            return Enumerable.Empty<StudentFeedbackDto>();
        }

        return feedbacksList.AsParallel()
            .Select(f => new StudentFeedbackDto
            {
                FeedbackId = f.Id,
                EventId = f.Booking?.EventId ?? Guid.Empty,
                EventTitle = f.Booking?.Event?.Title ?? "N/A",
                GeneralComment = f.GeneralComment,
                SubmittedAt = f.SubmittedAt,
                CriteriaScores = f.FeedbackDetails.ToDictionary(fd => fd.Criteria, fd => fd.Score),
                AverageScore = f.FeedbackDetails.Any() ? Math.Round(f.FeedbackDetails.Average(fd => fd.Score), 1) : 0.0
            })
            .ToList();
    }
}
