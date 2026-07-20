using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BLL.DTOs;

namespace BLL.Interfaces;

public interface IVenueService
{
    Task<IEnumerable<VenueDTO>> GetAllVenuesAsync(CancellationToken cancellationToken = default);
    Task<VenueDTO?> GetVenueByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<VenueDTO> CreateVenueAsync(VenueCreateDTO dto, CancellationToken cancellationToken = default);
    Task UpdateVenueAsync(VenueUpdateDTO dto, CancellationToken cancellationToken = default);
    Task DeleteVenueAsync(int id, CancellationToken cancellationToken = default);
}
