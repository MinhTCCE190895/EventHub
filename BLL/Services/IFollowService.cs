using System;
using System.Threading;
using System.Threading.Tasks;

namespace BLL.Services;

public interface IFollowService
{
    Task<bool> ToggleFollowAsync(Guid followerId, Guid followeeId, CancellationToken cancellationToken = default);
    Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId, CancellationToken cancellationToken = default);
    Task<int> GetFollowersCountAsync(Guid organizerId, CancellationToken cancellationToken = default);
}
