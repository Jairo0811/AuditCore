using AuditCore.Application.Common.Interfaces;
using AuditCore.Application.Features.Audits;
using AuditCore.Application.Features.Audits.Models;
using AuditCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditCore.Infrastructure.Services;

public sealed class AuditService : IAuditService
{
    private readonly IAuditCoreDbContext _dbContext;

    public AuditService(
        IAuditCoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<AuditDto>> GetAllAsync(
        Guid? organizationId = null,
        AuditStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Audits
            .AsNoTracking()
            .AsQueryable();

        if (organizationId.HasValue)
        {
            query = query.Where(
                x => x.OrganizationId == organizationId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(
                x => x.Status == status.Value);
        }

        return await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new AuditDto(
                x.Id,
                x.OrganizationId,
                x.Organization.Name,
                x.Code,
                x.Title,
                x.Objective,
                x.Scope,
                x.LeadAuditorUserId,
                x.LeadAuditorUser != null
                    ? x.LeadAuditorUser.FirstName + " " +
                      x.LeadAuditorUser.LastName
                    : null,
                x.StartDateUtc,
                x.EndDateUtc,
                x.Status,
                x.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<AuditDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Audits
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AuditDto(
                x.Id,
                x.OrganizationId,
                x.Organization.Name,
                x.Code,
                x.Title,
                x.Objective,
                x.Scope,
                x.LeadAuditorUserId,
                x.LeadAuditorUser != null
                    ? x.LeadAuditorUser.FirstName + " " +
                      x.LeadAuditorUser.LastName
                    : null,
                x.StartDateUtc,
                x.EndDateUtc,
                x.Status,
                x.IsActive))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<AuditDto> CreateAsync(
        CreateAuditRequest request,
        CancellationToken cancellationToken = default)
    {
        var organizationExists =
            await _dbContext.Organizations.AnyAsync(
                x => x.Id == request.OrganizationId,
                cancellationToken);

        if (!organizationExists)
        {
            throw new InvalidOperationException(
                "La organización indicada no existe.");
        }

        var code = NormalizeCode(request.Code);

        var duplicate =
            await _dbContext.Audits.AnyAsync(
                x => x.OrganizationId == request.OrganizationId &&
                     x.Code == code,
                cancellationToken);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"Ya existe una auditoría con el código '{code}' en esta organización.");
        }

        var audit = new Audit(
            request.OrganizationId,
            code,
            request.Title,
            request.Objective,
            request.Scope);

        _dbContext.Audits.Add(audit);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(
                   audit.Id,
                   cancellationToken)
               ?? throw new InvalidOperationException(
                   "No fue posible recuperar la auditoría creada.");
    }

    public async Task<AuditDto?> UpdateAsync(
        Guid id,
        UpdateAuditRequest request,
        CancellationToken cancellationToken = default)
    {
        var audit = await _dbContext.Audits
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (audit is null)
        {
            return null;
        }

        var code = NormalizeCode(request.Code);

        var duplicate =
            await _dbContext.Audits.AnyAsync(
                x => x.Id != id &&
                     x.OrganizationId == audit.OrganizationId &&
                     x.Code == code,
                cancellationToken);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"Ya existe otra auditoría con el código '{code}' en esta organización.");
        }

        audit.Update(
            code,
            request.Title,
            request.Objective,
            request.Scope);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<AuditDto?> PlanAsync(
        Guid id,
        PlanAuditRequest request,
        CancellationToken cancellationToken = default)
    {
        var audit = await _dbContext.Audits
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (audit is null)
        {
            return null;
        }

        var auditor = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == request.LeadAuditorUserId,
                cancellationToken);

        if (auditor is null)
        {
            throw new InvalidOperationException(
                "El auditor principal indicado no existe.");
        }

        if (auditor.OrganizationId != audit.OrganizationId)
        {
            throw new InvalidOperationException(
                "El auditor principal debe pertenecer a la organización de la auditoría.");
        }

        if (!auditor.IsActive || auditor.IsLocked)
        {
            throw new InvalidOperationException(
                "El auditor principal debe estar activo y desbloqueado.");
        }

        audit.Plan(
            request.LeadAuditorUserId,
            request.StartDateUtc,
            request.EndDateUtc);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public Task<bool> StartAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return ChangeStateAsync(
            id,
            audit => audit.Start(),
            cancellationToken);
    }

    public Task<bool> CompleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return ChangeStateAsync(
            id,
            audit => audit.Complete(),
            cancellationToken);
    }

    public Task<bool> CloseAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return ChangeStateAsync(
            id,
            audit => audit.Close(),
            cancellationToken);
    }

    public Task<bool> CancelAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return ChangeStateAsync(
            id,
            audit => audit.Cancel(),
            cancellationToken);
    }

    private async Task<bool> ChangeStateAsync(
        Guid id,
        Action<Audit> changeState,
        CancellationToken cancellationToken)
    {
        var audit = await _dbContext.Audits
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (audit is null)
        {
            return false;
        }

        changeState(audit);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return true;
    }

    private static string NormalizeCode(
        string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        return code.Trim().ToUpperInvariant();
    }
}
