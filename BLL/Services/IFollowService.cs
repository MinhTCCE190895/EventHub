using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BusinessObjects.DTOs;

namespace BLL.Services;

public interface IFollowService
{
    Task<IEnumerable<OrganizerDto>> GetOrganizersWithFollowCountAsync(Guid currentUserId);
    Task<bool> IsFollowingAsync(Guid followerId, Guid followeeId);
    Task FollowAsync(Guid followerId, Guid followeeId);
    Task UnfollowAsync(Guid followerId, Guid followeeId);
    Task<IEnumerable<OrganizerDto>> GetFollowedOrganizersAsync(Guid followerId);
    Task<IEnumerable<EventCardDTO>> GetNewEventsFromFollowedOrganizersAsync(Guid followerId);
}
