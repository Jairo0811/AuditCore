using AuditCore.Application.Features.Audits.Models;
using AuditCore.Domain.Entities;

namespace AuditCore.Application.Features.Audits;

public interface IAuditService
{
    Task<IReadOnlyCollection<AuditDto>> GetAllAsync(
        Guid? organizationId = null,
        AuditStatus? status = null,
        CancellationToken cancellationToken = default);

    Task<AuditDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<AuditDto> CreateAsync(
        CreateAuditRequest request,
        CancellationToken cancellationToken = default);

    Task<AuditDto?> UpdateAsync(
        Guid id,
        UpdateAuditRequest request,
        CancellationToken cancellationToken = default);

    Task<AuditDto?> PlanAsync(
        Guid id,
        PlanAuditRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> StartAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> CompleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> CloseAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> CancelAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
