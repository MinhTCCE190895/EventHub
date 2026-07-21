using BLL.Interfaces;
using AutoMapper;
using BLL.DTOs;
using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BLL.Services;

public class FollowService : IFollowService
{
    private readonly AppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<FollowService> _logger;

    public FollowService(AppDbContext context, IMapper mapper, ILogger<FollowService> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    // Lấy danh sách tất cả các đơn vị tổ chức kèm số lượng người theo dõi.
    // Lọc user có vai trò "Organizer" và đang hoạt động -> Đếm tổng số follower -> Kiểm tra user hiện tại có follow chưa -> Sắp xếp giảm dần theo lượng follow.
    public async Task<IEnumerable<OrganizerDto>> GetOrganizersWithFollowCountAsync(Guid currentUserId)
    {
        _logger.LogInformation("Getting organizers with follow count for user {UserId}", currentUserId);
        return await _context.Users
            .Where(u => u.Role == "Organizer" && u.IsActive)
            .Select(u => new OrganizerDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                FollowCount = u.Followers.Count(),
                IsFollowed = u.Followers.Any(f => f.FollowerId == currentUserId)
            })
            .OrderByDescending(o => o.FollowCount)
            .ThenBy(o => o.FullName)
            .ToListAsync();
    }

    // Kiểm tra xem một người dùng có đang theo dõi một đơn vị tổ chức hay không.
    // Dùng AnyAsync kiểm tra xem có bản ghi trùng khớp cặp mã FollowerId và FolloweeId trong bảng Follows không.
    public async Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId)
    {
        return await _context.Follows.AnyAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);
    }

    // Thực hiện theo dõi một đơn vị tổ chức.
    // Kiểm tra đối tượng có phải Organizer đang hoạt động không -> Kiểm tra nếu chưa từng theo dõi thì thêm bản ghi mới vào bảng Follows -> Lưu xuống DB.
    public async Task FollowAsync(Guid followerId, Guid followeeId)
    {
        _logger.LogInformation("User {FollowerId} attempting to follow organizer {FolloweeId}", followerId, followeeId);
        
        var isOrganizer = await _context.Users.AnyAsync(u => u.Id == followeeId && u.Role == "Organizer" && u.IsActive);
        if (!isOrganizer)
        {
            throw new ArgumentException("Target user is not an active organizer.");
        }

        var exists = await _context.Follows.AnyAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);
        if (!exists)
        {
            var follow = new Follow
            {
                FollowerId = followerId,
                FolloweeId = followeeId,
                FollowedAt = DateTime.UtcNow
            };
            await _context.Follows.AddAsync(follow);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User {FollowerId} successfully followed organizer {FolloweeId}", followerId, followeeId);
        }
    }

    // Hủy theo dõi một đơn vị tổ chức.
    // Tìm bản ghi theo dõi tương ứng trong bảng Follows -> Nếu có thì tiến hành xóa bản ghi đó và lưu thay đổi xuống DB.
    public async Task UnfollowAsync(Guid followerId, Guid followeeId)
    {
        _logger.LogInformation("User {FollowerId} attempting to unfollow organizer {FolloweeId}", followerId, followeeId);
        
        var follow = await _context.Follows.FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);
        if (follow != null)
        {
            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User {FollowerId} successfully unfollowed organizer {FolloweeId}", followerId, followeeId);
        }
    }

    // Lấy danh sách các đơn vị tổ chức mà người dùng hiện tại đang theo dõi.
    // Lọc bảng Follows theo mã người dùng -> Map thông tin của đơn vị tổ chức sang DTO kèm tính toán số lượng follow -> Sắp xếp theo lượng follow giảm dần.
    public async Task<IEnumerable<OrganizerDto>> GetFollowedOrganizersAsync(Guid followerId)
    {
        _logger.LogInformation("Getting followed organizers for user {FollowerId}", followerId);
        return await _context.Follows
            .Where(f => f.FollowerId == followerId && f.Followee.IsActive)
            .Select(f => new OrganizerDto
            {
                Id = f.FolloweeId,
                FullName = f.Followee.FullName,
                Email = f.Followee.Email,
                FollowCount = f.Followee.Followers.Count(),
                IsFollowed = true
            })
            .OrderByDescending(o => o.FollowCount)
            .ThenBy(o => o.FullName)
            .ToListAsync();
    }

    // Lấy danh sách các sự kiện mới nhất từ các đơn vị tổ chức đã theo dõi.
    // Tìm danh sách Id của các đơn vị được follow -> Lọc các sự kiện thuộc danh sách Id đó ở trạng thái "Published" -> Sắp xếp theo thời gian và dùng AutoMapper để chuyển sang DTO.
    public async Task<IEnumerable<EventCardDTO>> GetNewEventsFromFollowedOrganizersAsync(Guid followerId)
    {
        _logger.LogInformation("Getting new events from followed organizers for user {FollowerId}", followerId);
        
        var followedOrganizerIds = await _context.Follows
            .Where(f => f.FollowerId == followerId)
            .Select(f => f.FolloweeId)
            .ToListAsync();

        if (!followedOrganizerIds.Any())
        {
            return Enumerable.Empty<EventCardDTO>();
        }

        var events = await _context.Events
            .Include(e => e.Venue)
            .Include(e => e.Organizer)
            .Include(e => e.EventTags)
                .ThenInclude(et => et.Tag)
            .Include(e => e.Bookings)
            .Where(e => followedOrganizerIds.Contains(e.OrganizerId) && e.Status == "Published")
            .OrderByDescending(e => e.StartTime)
            .ToListAsync();

        return _mapper.Map<List<EventCardDTO>>(events);
    }
}