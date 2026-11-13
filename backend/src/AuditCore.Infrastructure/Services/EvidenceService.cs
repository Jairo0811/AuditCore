using System.Security.Cryptography;
using AuditCore.Application.Common.Interfaces;
using AuditCore.Application.Common.Security;
using AuditCore.Application.Features.Evidence;
using AuditCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AuditCore.Infrastructure.Services;

public sealed class EvidenceService : IEvidenceService
{
    public const long MaxFileSizeBytes = 20 * 1024 * 1024;

    private readonly IAuditCoreDbContext _dbContext;
    private readonly TenantGuard _tenantGuard;
    private readonly ICurrentUserContext _currentUser;
    private readonly string _storageRoot;

    public EvidenceService(IAuditCoreDbContext dbContext, TenantGuard tenantGuard, ICurrentUserContext currentUser, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
        _currentUser = currentUser;
        _storageRoot = Path.TrimEndingDirectorySeparator(
            Path.GetFullPath(configuration["EvidenceStorage:Path"] ?? Path.Combine(AppContext.BaseDirectory, "evidence-storage")));
        Directory.CreateDirectory(_storageRoot);
    }

    public async Task<IReadOnlyCollection<EvidenceDto>> GetAllAsync(Guid? auditId = null, Guid? findingId = null, CancellationToken cancellationToken = default)
    {
        var query = VisibleEvidence();
        if (auditId.HasValue) query = query.Where(x => x.AuditId == auditId.Value);
        if (findingId.HasValue) query = query.Where(x => x.FindingId == findingId.Value);
        return await query.OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new EvidenceDto(x.Id, x.AuditId, x.FindingId, x.FileName, x.ContentType, x.SizeBytes, x.Sha256, x.Description, x.UploadedByUserId, x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<EvidenceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await VisibleEvidence().Where(x => x.Id == id)
            .Select(x => new EvidenceDto(x.Id, x.AuditId, x.FindingId, x.FileName, x.ContentType, x.SizeBytes, x.Sha256, x.Description, x.UploadedByUserId, x.CreatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<EvidenceDto> CreateAsync(CreateEvidenceRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Content.Length == 0 || request.Content.LongLength > MaxFileSizeBytes)
            throw new InvalidOperationException("La evidencia debe tener entre 1 byte y 20 MB.");

        var safeName = Path.GetFileName((request.FileName ?? string.Empty).Replace('\\', '/')).Trim();
        if (string.IsNullOrWhiteSpace(safeName) || safeName.Length > 255)
            throw new InvalidOperationException("El nombre de archivo no es válido.");

        var contentType = EvidenceFileRules.ResolveContentType(safeName, request.ContentType, request.Content);

        var audit = await _dbContext.Audits.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.AuditId, cancellationToken)
            ?? throw new InvalidOperationException("La auditoría indicada no existe.");
        _tenantGuard.EnsureOrganization(audit.OrganizationId);

        if (request.FindingId.HasValue)
        {
            var belongs = await _dbContext.Findings.AsNoTracking().AnyAsync(x => x.Id == request.FindingId.Value && x.AuditId == request.AuditId, cancellationToken);
            if (!belongs) throw new InvalidOperationException("El hallazgo no pertenece a la auditoría indicada.");
        }

        // La autoría siempre corresponde al actor autenticado; el cliente no puede atribuirla a otro usuario.
        var uploadedByUserId = _currentUser.UserId
            ?? throw new UnauthorizedAccessException("No fue posible identificar al usuario que carga la evidencia.");

        var hash = Convert.ToHexString(SHA256.HashData(request.Content));
        var extension = Path.GetExtension(safeName).ToLowerInvariant();
        var storageKey = $"{request.AuditId:N}/{Guid.NewGuid():N}{extension}";
        var fullPath = GetSafePath(storageKey);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllBytesAsync(fullPath, request.Content, cancellationToken);

        try
        {
            var evidence = new Evidence(request.AuditId, safeName, contentType, request.Content.LongLength, storageKey, hash, request.Description, request.FindingId, uploadedByUserId);
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
        var entity = await VisibleEvidence().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (entity is null) return null;
        var path = GetSafePath(entity.StorageKey);
        if (!File.Exists(path)) throw new FileNotFoundException("El archivo físico de la evidencia no existe.");
        var content = await File.ReadAllBytesAsync(path, cancellationToken);
        return (new EvidenceDto(entity.Id, entity.AuditId, entity.FindingId, entity.FileName, entity.ContentType, entity.SizeBytes, entity.Sha256, entity.Description, entity.UploadedByUserId, entity.CreatedAtUtc), content);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Evidences.Include(x => x.Audit)
            .SingleOrDefaultAsync(x => x.Id == id && x.IsActive, cancellationToken);
        if (entity is null) return false;
        _tenantGuard.EnsureOrganization(entity.Audit.OrganizationId);
        var path = GetSafePath(entity.StorageKey);

        // Se conserva el registro para trazabilidad, pero queda inactivo y marcado como eliminado:
        // el filtro global (IsDeleted) lo excluye de listados, consultas y descargas.
        entity.Deactivate();
        entity.SoftDelete();
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (File.Exists(path)) File.Delete(path);
        return true;
    }

    /// <summary>
    /// Evidencias activas visibles para el usuario actual. Incluye el filtro explícito por IsActive para
    /// excluir también registros desactivados antes de que la eliminación aplicara soft delete.
    /// </summary>
    private IQueryable<Evidence> VisibleEvidence()
    {
        var query = _dbContext.Evidences.AsNoTracking().Where(x => x.IsActive);
        var restricted = _tenantGuard.RestrictedOrganizationId;
        if (restricted.HasValue) query = query.Where(x => x.Audit.OrganizationId == restricted.Value);
        return query;
    }

    private string GetSafePath(string storageKey) => EvidenceFileRules.ResolveSafePath(_storageRoot, storageKey);
}
