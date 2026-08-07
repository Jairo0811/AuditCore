using System.Security.Cryptography;
using AuditCore.Application.Common.Interfaces;
using AuditCore.Application.Features.Evidence;
using AuditCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AuditCore.Infrastructure.Services;

public sealed class EvidenceService : IEvidenceService
{
    private const long MaxFileSizeBytes = 20 * 1024 * 1024;
    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/png",
        "image/jpeg",
        "text/plain",
        "text/csv",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    };

    private readonly IAuditCoreDbContext _dbContext;
    private readonly string _storageRoot;

    public EvidenceService(IAuditCoreDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _storageRoot = Path.GetFullPath(configuration["EvidenceStorage:Path"] ?? Path.Combine(AppContext.BaseDirectory, "evidence-storage"));
        Directory.CreateDirectory(_storageRoot);
    }

    public async Task<IReadOnlyCollection<EvidenceDto>> GetAllAsync(Guid? auditId = null, Guid? findingId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Evidences.AsNoTracking().AsQueryable();
        if (auditId.HasValue) query = query.Where(x => x.AuditId == auditId.Value);
        if (findingId.HasValue) query = query.Where(x => x.FindingId == findingId.Value);

        return await query.OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new EvidenceDto(x.Id, x.AuditId, x.FindingId, x.FileName, x.ContentType, x.SizeBytes, x.Sha256, x.Description, x.UploadedByUserId, x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<EvidenceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _dbContext.Evidences.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new EvidenceDto(x.Id, x.AuditId, x.FindingId, x.FileName, x.ContentType, x.SizeBytes, x.Sha256, x.Description, x.UploadedByUserId, x.CreatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<EvidenceDto> CreateAsync(CreateEvidenceRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Content.Length == 0 || request.Content.LongLength > MaxFileSizeBytes)
            throw new InvalidOperationException("La evidencia debe tener entre 1 byte y 20 MB.");
        if (!AllowedContentTypes.Contains(request.ContentType))
            throw new InvalidOperationException("El tipo de archivo no está permitido.");

        var audit = await _dbContext.Audits.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.AuditId, cancellationToken)
            ?? throw new InvalidOperationException("La auditoría indicada no existe.");

        if (request.FindingId.HasValue)
        {
            var belongs = await _dbContext.Findings.AsNoTracking().AnyAsync(x => x.Id == request.FindingId.Value && x.AuditId == request.AuditId, cancellationToken);
            if (!belongs) throw new InvalidOperationException("El hallazgo no pertenece a la auditoría indicada.");
        }

        if (request.UploadedByUserId.HasValue)
        {
            var validUser = await _dbContext.Users.AsNoTracking().AnyAsync(x => x.Id == request.UploadedByUserId.Value && x.OrganizationId == audit.OrganizationId && x.IsActive && !x.IsLocked, cancellationToken);
            if (!validUser) throw new InvalidOperationException("El usuario que carga la evidencia no es válido para esta organización.");
        }

        var safeName = Path.GetFileName(request.FileName);
        if (string.IsNullOrWhiteSpace(safeName)) throw new InvalidOperationException("El nombre de archivo no es válido.");

        var hash = Convert.ToHexString(SHA256.HashData(request.Content));
        var extension = Path.GetExtension(safeName);
        var storageKey = $"{request.AuditId:N}/{Guid.NewGuid():N}{extension}";
        var fullPath = GetSafePath(storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllBytesAsync(fullPath, request.Content, cancellationToken);

        try
        {
            var evidence = new Evidence(request.AuditId, safeName, request.ContentType, request.Content.LongLength, storageKey, hash, request.Description, request.FindingId, request.UploadedByUserId);
            _dbContext.Evidences.Add(evidence);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return (await GetByIdAsync(evidence.Id, cancellationToken))!;
        }
        catch
        {
            if (File.Exists(fullPath)) File.Delete(fullPath);
            throw;
        }
    }

    public async Task<(EvidenceDto Metadata, byte[] Content)?> DownloadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Evidences.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return null;
        var path = GetSafePath(entity.StorageKey);
        if (!File.Exists(path)) throw new FileNotFoundException("El archivo físico de la evidencia no existe.");
        var content = await File.ReadAllBytesAsync(path, cancellationToken);
        var dto = new EvidenceDto(entity.Id, entity.AuditId, entity.FindingId, entity.FileName, entity.ContentType, entity.SizeBytes, entity.Sha256, entity.Description, entity.UploadedByUserId, entity.CreatedAtUtc);
        return (dto, content);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Evidences.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return false;
        var path = GetSafePath(entity.StorageKey);
        entity.Deactivate();
        await _dbContext.SaveChangesAsync(cancellationToken);
        if (File.Exists(path)) File.Delete(path);
        return true;
    }

    private string GetSafePath(string storageKey)
    {
        var fullPath = Path.GetFullPath(Path.Combine(_storageRoot, storageKey.Replace('/', Path.DirectorySeparatorChar)));
        if (!fullPath.StartsWith(_storageRoot, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Ruta de almacenamiento inválida.");
        return fullPath;
    }
}
