using AuditCore.Application.Features.Organizations.Models;

namespace AuditCore.Application.Features.Organizations;

public interface IOrganizationService
{
    Task<IReadOnlyCollection<OrganizationDto>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<OrganizationDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<OrganizationDto> CreateAsync(
        CreateOrganizationRequest request,
        CancellationToken cancellationToken = default);

    Task<OrganizationDto?> UpdateAsync(
        Guid id,
        UpdateOrganizationRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
