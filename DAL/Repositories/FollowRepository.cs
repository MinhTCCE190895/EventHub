using DAL.Data;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DAL.Repositories;

public class FollowRepository : BaseRepository<Follow>, IFollowRepository
{
    public FollowRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<int> GetFollowersCountAsync(Guid organizerId, CancellationToken cancellationToken = default)
    {
        // FE-12: optimized count directly on the database without loading all data into RAM
        return await _dbSet.CountAsync(f => f.FolloweeId == organizerId, cancellationToken);
    }

    public async Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(f => f.FollowerId == followerId && f.FolloweeId == followeeId, cancellationToken);
    }
}
