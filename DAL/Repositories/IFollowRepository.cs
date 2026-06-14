using DAL.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DAL.Repositories;

public interface IFollowRepository : IRepository<Follow>
{
    Task<int> GetFollowersCountAsync(Guid organizerId, CancellationToken cancellationToken = default);
    Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId, CancellationToken cancellationToken = default);
}
