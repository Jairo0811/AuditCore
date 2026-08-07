using AuditCore.Application.Features.Risks.Models;
using AuditCore.Domain.Entities;

namespace AuditCore.Application.Features.Risks;

public interface IRiskService
{
    Task<IReadOnlyCollection<RiskDto>> GetAllAsync(
        Guid? auditId = null,
        RiskStatus? status = null,
        RiskLevel? level = null,
        CancellationToken cancellationToken = default);

    Task<RiskDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<RiskDto> CreateAsync(
        CreateRiskRequest request,
        CancellationToken cancellationToken = default);

    Task<RiskDto?> UpdateAsync(
        Guid id,
        UpdateRiskRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> StartTreatmentAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> AcceptAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> MitigateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> CloseAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
