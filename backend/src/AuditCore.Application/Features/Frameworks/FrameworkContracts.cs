using AuditCore.Domain.Entities;

namespace AuditCore.Application.Features.Frameworks;

public sealed record FrameworkDto(Guid Id, string Name, string Code, string Version, string? Description, bool IsActive);
public sealed record ControlDto(Guid Id, Guid FrameworkId, string Code, string Title, string Domain, string? Description, decimal Weight, bool IsActive);
public sealed record EvaluationDto(Guid Id, Guid AuditId, Guid ControlId, string ControlCode, int? Score, ComplianceStatus Status, string? Notes, Guid? EvaluatedByUserId, DateTime? EvaluatedAtUtc);

public sealed record CreateFrameworkRequest(string Name, string Code, string Version, string? Description);
public sealed record UpdateFrameworkRequest(string Name, string Code, string Version, string? Description, bool IsActive);
public sealed record CreateControlRequest(Guid FrameworkId, string Code, string Title, string Domain, decimal Weight, string? Description);
public sealed record UpdateControlRequest(string Code, string Title, string Domain, decimal Weight, string? Description, bool IsActive);
public sealed record EvaluateControlRequest(int? Score, ComplianceStatus Status, string? Notes, Guid EvaluatedByUserId);

public interface IFrameworkService
{
    Task<IReadOnlyCollection<FrameworkDto>> GetFrameworksAsync(CancellationToken cancellationToken = default);
    Task<FrameworkDto> CreateFrameworkAsync(CreateFrameworkRequest request, CancellationToken cancellationToken = default);
    Task<FrameworkDto?> UpdateFrameworkAsync(Guid id, UpdateFrameworkRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ControlDto>> GetControlsAsync(Guid? frameworkId = null, CancellationToken cancellationToken = default);
    Task<ControlDto> CreateControlAsync(CreateControlRequest request, CancellationToken cancellationToken = default);
    Task<ControlDto?> UpdateControlAsync(Guid id, UpdateControlRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<EvaluationDto>> GetEvaluationsAsync(Guid auditId, CancellationToken cancellationToken = default);
    Task<EvaluationDto> EvaluateAsync(Guid auditId, Guid controlId, EvaluateControlRequest request, CancellationToken cancellationToken = default);
}
