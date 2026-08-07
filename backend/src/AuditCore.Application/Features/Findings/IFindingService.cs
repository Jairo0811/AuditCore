using AuditCore.Application.Features.Findings.Models;
using AuditCore.Domain.Entities;

namespace AuditCore.Application.Features.Findings;

public interface IFindingService
{
    Task<IReadOnlyCollection<FindingDto>> GetAllAsync(
        Guid? auditId = null,
        FindingStatus? status = null,
        FindingSeverity? severity = null,
        CancellationToken cancellationToken = default);

    Task<FindingDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<FindingDto> CreateAsync(
        CreateFindingRequest request,
        CancellationToken cancellationToken = default);

    Task<FindingDto?> UpdateAsync(
        Guid id,
        UpdateFindingRequest request,
        CancellationToken cancellationToken = default);

    Task<bool> SendToReviewAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> AcceptAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> ResolveAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<bool> CloseAsync(
        Guid id,
        CancellationToken cancellationToken = default);
}
