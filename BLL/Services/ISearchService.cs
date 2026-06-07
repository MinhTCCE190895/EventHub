using BusinessObjects.DTOs;

namespace BLL.Services;

public interface ISearchService
{
    Task<(List<EventCardDTO> Items, int TotalCount)> SearchEventsAsync(
        EventSearchDTO searchDto,
        CancellationToken cancellationToken = default);
}
