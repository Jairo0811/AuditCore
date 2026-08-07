namespace AuditCore.Application.Features.Evidence;

public sealed record EvidenceDto(
    Guid Id,
    Guid AuditId,
    Guid? FindingId,
    string FileName,
    string ContentType,
    long SizeBytes,
    string Sha256,
    string? Description,
    Guid? UploadedByUserId,
    DateTime CreatedAtUtc);

public sealed record CreateEvidenceRequest(
    Guid AuditId,
    Guid? FindingId,
    string FileName,
    string ContentType,
    byte[] Content,
    string? Description,
    Guid? UploadedByUserId);

public interface IEvidenceService
{
    Task<IReadOnlyCollection<EvidenceDto>> GetAllAsync(Guid? auditId = null, Guid? findingId = null, CancellationToken cancellationToken = default);
    Task<EvidenceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<EvidenceDto> CreateAsync(CreateEvidenceRequest request, CancellationToken cancellationToken = default);
    Task<(EvidenceDto Metadata, byte[] Content)?> DownloadAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
