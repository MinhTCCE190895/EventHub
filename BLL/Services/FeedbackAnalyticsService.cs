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


    // Kiểm tra sinh viên có được phép gửi đánh giá cho sự kiện hay không.
    // Tìm vé của sinh viên đó, nếu trạng thái là "Confirmed" và chưa có Feedback gắn kèm thì hợp lệ.
    public async Task<bool> CanSubmitFeedbackAsync(Guid eventId, Guid studentId)
    {
        var booking = await _bookingRepository.Query()
            .Include(b => b.Feedback)
            .FirstOrDefaultAsync(b => b.EventId == eventId && b.StudentId == studentId && b.Status == "Confirmed");

        if (booking == null) return false;
        
        // Nếu đã có đánh giá rồi thì không cho phép đánh giá tiếp
        return booking.Feedback == null;
    }


    // Lưu thông tin phản hồi và điểm số chi tiết của sinh viên vào Database.
    // Kiểm tra mã đặt vé -> Tạo đối tượng Feedback -> Duyệt danh sách điểm để add vào FeedbackDetails -> Lưu qua DbContext.
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


    // Tính toán các chỉ số thống kê (Điểm tiêu chí, điểm tổng quan) của từng sự kiện.
    // Chạy GroupBy trực tiếp dưới SQL để tính trung bình tiêu chí và tổng số feedback, sau đó map kết quả vào Dictionary để tối ưu tốc độ trả về.
    public async Task<IEnumerable<EventFeedbackMetricsDto>> GetFeedbackMetricsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(">>> [SQL Engine] Khởi chạy tính toán thống kê Feedback qua LINQ SQL GroupBy...");

        // Bước 1: Tính toán điểm trung bình từng tiêu chí (Criteria) gom nhóm trực tiếp từ Database
        var details = await _context.FeedbackDetails
            .Where(fd => fd.Feedback.Booking != null && fd.Feedback.Booking.Event != null)
            .GroupBy(fd => new { fd.Feedback.Booking.EventId, fd.Feedback.Booking.Event.Title, fd.Criteria })
            .Select(g => new
            {
                EventId = g.Key.EventId,
                EventTitle = g.Key.Title,
                Criteria = g.Key.Criteria,
                AverageScore = g.Average(fd => (double)fd.Score)
            })
            .ToListAsync(cancellationToken);

        // Bước 2: Thống kê số lượng phản hồi và điểm trung bình tổng quan trực tiếp dưới Database
        var counts = await _context.Feedbacks
            .Where(f => f.Booking != null && f.Booking.Event != null)
            .GroupBy(f => new { f.Booking.EventId, f.Booking.Event.Title })
            .Select(g => new
            {
                EventId = g.Key.EventId,
                EventTitle = g.Key.Title,
                TotalFeedbacks = g.Count(),
                OverallAverageScore = g.SelectMany(f => f.FeedbackDetails).Average(fd => (double)fd.Score)
            })
            .ToListAsync(cancellationToken);

        var detailsGrouped = details.GroupBy(d => d.EventId).ToDictionary(g => g.Key, g => g.ToList());

        var metrics = counts.Select(c =>
        {
            var eventDetails = detailsGrouped.TryGetValue(c.EventId, out var dList) ? dList : new();
            return new EventFeedbackMetricsDto
            {
                EventId = c.EventId,
                EventTitle = c.EventTitle,
                TotalFeedbacks = c.TotalFeedbacks,
                OverallAverageScore = Math.Round(c.OverallAverageScore, 1),
                CriteriaAverages = eventDetails.ToDictionary(
                    ed => ed.Criteria,
                    ed => Math.Round(ed.AverageScore, 1)
                )
            };
        }).ToList();

        _logger.LogInformation(">>> [SQL Engine] Hoàn tất tính toán thống kê phản hồi cho {Count} sự kiện.", metrics.Count);
        return metrics;
    }

    // Lấy toàn bộ danh sách đánh giá chi tiết của một sự kiện cụ thể.
    // Tải dữ liệu từ DB lên RAM, sử dụng PLINQ (.AsParallel()) để chia nhỏ danh sách và tính điểm trung bình song song nhằm tăng hiệu năng.
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

    // Lấy lịch sử tất cả các đánh giá mà một sinh viên đã gửi.
    // Lấy danh sách từ Repo theo StudentId, sau đó dùng PLINQ map sang DTO và tính điểm trung bình cho từng feedback.
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
