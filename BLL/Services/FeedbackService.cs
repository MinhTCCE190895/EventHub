using DAL.Data;
using DAL.Entities;
using DAL.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BLL.Services;

public class FeedbackService : IFeedbackService
{
    private readonly IRepository<Booking> _bookingRepository;
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly AppDbContext _context;

    public FeedbackService(
        IRepository<Booking> bookingRepository, 
        IFeedbackRepository feedbackRepository,
        AppDbContext context)
    {
        _bookingRepository = bookingRepository;
        _feedbackRepository = feedbackRepository;
        _context = context;
    }

    public async Task<bool> CanUserLeaveFeedbackAsync(Guid userId, Guid eventId, CancellationToken cancellationToken = default)
    {
        // Check if student has a Confirmed booking for the event
        var booking = await _bookingRepository.SingleOrDefaultAsync(b => 
            b.StudentId == userId && b.EventId == eventId && b.Status == "Confirmed", 
            cancellationToken);

        if (booking == null) return false;

        // Check if they have already submitted feedback
        var alreadyFeedbacked = await _feedbackRepository.ExistsAsync(f => f.BookingId == booking.Id, cancellationToken);
        return !alreadyFeedbacked;
    }

    public async Task SubmitFeedbackAsync(
        Guid userId, 
        Guid eventId, 
        string? generalComment, 
        List<(string Criteria, int Score)> criteriaScores, 
        CancellationToken cancellationToken = default)
    {
        var booking = await _bookingRepository.SingleOrDefaultAsync(b => 
            b.StudentId == userId && b.EventId == eventId && b.Status == "Confirmed", 
            cancellationToken);

        if (booking == null)
        {
            throw new InvalidOperationException("Sinh viên không có vé xác nhận (Confirmed) cho sự kiện này để gửi đánh giá.");
        }

        var alreadyFeedbacked = await _feedbackRepository.ExistsAsync(f => f.BookingId == booking.Id, cancellationToken);
        if (alreadyFeedbacked)
        {
            throw new InvalidOperationException("Bạn đã gửi đánh giá cho sự kiện này rồi.");
        }

        var feedback = new Feedback
        {
            Id = Guid.NewGuid(),
            BookingId = booking.Id,
            GeneralComment = generalComment,
            SubmittedAt = DateTime.UtcNow
        };

        foreach (var (criteria, score) in criteriaScores)
        {
            if (score < 1 || score > 5)
            {
                throw new ArgumentException("Điểm đánh giá phải nằm trong khoảng từ 1 đến 5.");
            }

            feedback.FeedbackDetails.Add(new FeedbackDetail
            {
                Criteria = criteria,
                Score = score
            });
        }

        await _feedbackRepository.AddAsync(feedback, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
