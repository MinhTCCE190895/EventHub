using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BusinessObjects.DTOs;

namespace BLL.Services;

public interface IOrganizerService
{
    Task<IEnumerable<OrganizerDTO>> GetAllOrganizersAsync(CancellationToken cancellationToken = default);
    Task<OrganizerDTO?> GetOrganizerByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OrganizerDTO> CreateOrganizerAsync(OrganizerCreateDTO dto, CancellationToken cancellationToken = default);
    Task UpdateOrganizerAsync(OrganizerUpdateDTO dto, CancellationToken cancellationToken = default);
    Task DeleteOrganizerAsync(Guid id, CancellationToken cancellationToken = default);
}
