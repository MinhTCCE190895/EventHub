using BusinessObjects.DTOs;

namespace BLL.Interfaces;

public interface ISearchService
{
    Task<(List<EventCardDTO> Items, int TotalCount)> SearchEventsAsync(
        EventSearchDTO searchDto,
        CancellationToken cancellationToken = default);
}
