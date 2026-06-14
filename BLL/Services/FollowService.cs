using DAL.Data;
using DAL.Entities;
using DAL.Repositories;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Services;

public class FollowService : IFollowService
{
    private readonly IFollowRepository _followRepository;
    private readonly AppDbContext _context;

    public FollowService(IFollowRepository followRepository, AppDbContext context)
    {
        _followRepository = followRepository;
        _context = context;
    }

    public async Task<bool> ToggleFollowAsync(Guid followerId, Guid followeeId, CancellationToken cancellationToken = default)
    {
        if (followerId == followeeId)
        {
            throw new InvalidOperationException("Bạn không thể tự theo dõi chính mình.");
        }

        var isFollowing = await _followRepository.IsFollowingAsync(followerId, followeeId, cancellationToken);
        if (isFollowing)
        {
            var follow = await _followRepository.SingleOrDefaultAsync(f => 
                f.FollowerId == followerId && f.FolloweeId == followeeId, 
                cancellationToken);
            if (follow != null)
            {
                _followRepository.Remove(follow);
                await _context.SaveChangesAsync(cancellationToken);
            }
            return false;
        }
        else
        {
            var follow = new Follow
            {
                FollowerId = followerId,
                FolloweeId = followeeId,
                FollowedAt = DateTime.UtcNow
            };
            await _followRepository.AddAsync(follow, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }

    public async Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId, CancellationToken cancellationToken = default)
    {
        return await _followRepository.IsFollowingAsync(followerId, followeeId, cancellationToken);
    }

    public async Task<int> GetFollowersCountAsync(Guid organizerId, CancellationToken cancellationToken = default)
    {
        return await _followRepository.GetFollowersCountAsync(organizerId, cancellationToken);
    }
}
