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

    // L?y danh sách t?t c? các don v? t? ch?c kèm s? lu?ng ngu?i theo dõi.
    // L?c user có vai trò "Organizer" và dang ho?t d?ng -> Ð?m t?ng s? follower -> Ki?m tra user hi?n t?i có follow chua -> S?p x?p gi?m d?n theo lu?ng follow.
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

    // Ki?m tra xem m?t ngu?i dùng có dang theo dõi m?t don v? t? ch?c hay không.
    // Dùng AnyAsync ki?m tra xem có b?n ghi trùng kh?p c?p mã FollowerId và FolloweeId trong b?ng Follows không.
    public async Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId)
    {
        return await _context.Follows.AnyAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId);
    }

    // Th?c hi?n theo dõi m?t don v? t? ch?c.
    // Ki?m tra d?i tu?ng có ph?i Organizer dang ho?t d?ng không -> Ki?m tra n?u chua t?ng theo dõi thì thêm b?n ghi m?i vào b?ng Follows -> Luu xu?ng DB.
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

    // H?y theo dõi m?t don v? t? ch?c.
    // Tìm b?n ghi theo dõi tuong ?ng trong b?ng Follows -> N?u có thì ti?n hành xóa b?n ghi dó và luu thay d?i xu?ng DB.
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

    // L?y danh sách các don v? t? ch?c mà ngu?i dùng hi?n t?i dang theo dõi.
    // L?c b?ng Follows theo mã ngu?i dùng -> Map thông tin c?a don v? t? ch?c sang DTO kèm tính toán s? lu?ng follow -> S?p x?p theo lu?ng follow gi?m d?n.
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

    // L?y danh sách các s? ki?n m?i nh?t t? các don v? t? ch?c dã theo dõi.
    // Tìm danh sách Id c?a các don v? du?c follow -> L?c các s? ki?n thu?c danh sách Id dó ? tr?ng thái "Published" -> S?p x?p theo th?i gian và dùng AutoMapper d? chuy?n sang DTO.
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
