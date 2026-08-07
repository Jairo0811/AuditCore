using AuditCore.Domain.Common;

namespace AuditCore.Domain.Entities;

public sealed class Evidence : BaseAuditableEntity
{
    private Evidence() { }

    public Evidence(
        Guid auditId,
        string fileName,
        string contentType,
        long sizeBytes,
        string storageKey,
        string sha256,
        string? description = null,
        Guid? findingId = null,
        Guid? uploadedByUserId = null)
    {
        if (auditId == Guid.Empty) throw new ArgumentException("La auditoría es obligatoria.", nameof(auditId));
        if (sizeBytes <= 0) throw new ArgumentOutOfRangeException(nameof(sizeBytes), "El archivo debe tener contenido.");

        AuditId = auditId;
        FindingId = findingId;
        UploadedByUserId = uploadedByUserId;
        FileName = Required(fileName, nameof(fileName));
        ContentType = Required(contentType, nameof(contentType));
        StorageKey = Required(storageKey, nameof(storageKey));
        Sha256 = Required(sha256, nameof(sha256)).ToUpperInvariant();
        Description = Optional(description);
        SizeBytes = sizeBytes;
        IsActive = true;
    }

    public Guid AuditId { get; private set; }
    public Audit Audit { get; private set; } = null!;
    public Guid? FindingId { get; private set; }
    public Finding? Finding { get; private set; }
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long SizeBytes { get; private set; }
    public string StorageKey { get; private set; } = string.Empty;
    public string Sha256 { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? UploadedByUserId { get; private set; }
    public User? UploadedByUser { get; private set; }
    public bool IsActive { get; private set; }

    public void UpdateDescription(string? description) => Description = Optional(description);
    public void Deactivate() => IsActive = false;

    private static string Required(string value, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, name);
        return value.Trim();
    }

    private static string? Optional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
