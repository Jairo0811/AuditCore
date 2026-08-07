using AuditCore.Application.Common.Interfaces;
using AuditCore.Application.Features.Risks;
using AuditCore.Application.Features.Risks.Models;
using AuditCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditCore.Infrastructure.Services;

public sealed class RiskService : IRiskService
{
    private readonly IAuditCoreDbContext _dbContext;

    public RiskService(
        IAuditCoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<RiskDto>> GetAllAsync(
        Guid? auditId = null,
        RiskStatus? status = null,
        RiskLevel? level = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Risks
            .AsNoTracking()
            .AsQueryable();

        if (auditId.HasValue)
        {
            query = query.Where(
                x => x.AuditId == auditId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(
                x => x.Status == status.Value);
        }

        var risks = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new RiskDto(
                x.Id,
                x.AuditId,
                x.Audit.Code,
                x.Code,
                x.Title,
                x.Description,
                x.Probability,
                x.Impact,
                x.Probability * x.Impact,
                CalculateLevel(x.Probability * x.Impact),
                x.Treatment,
                x.OwnerUserId,
                x.OwnerUser != null
                    ? x.OwnerUser.FirstName + " " +
                      x.OwnerUser.LastName
                    : null,
                x.Status,
                x.IsActive))
            .ToListAsync(cancellationToken);

        if (!level.HasValue)
        {
            return risks;
        }

        return risks
            .Where(x => x.Level == level.Value)
            .ToArray();
    }

    public async Task<RiskDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Risks
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new RiskDto(
                x.Id,
                x.AuditId,
                x.Audit.Code,
                x.Code,
                x.Title,
                x.Description,
                x.Probability,
                x.Impact,
                x.Probability * x.Impact,
                CalculateLevel(x.Probability * x.Impact),
                x.Treatment,
                x.OwnerUserId,
                x.OwnerUser != null
                    ? x.OwnerUser.FirstName + " " +
                      x.OwnerUser.LastName
                    : null,
                x.Status,
                x.IsActive))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<RiskDto> CreateAsync(
        CreateRiskRequest request,
        CancellationToken cancellationToken = default)
    {
        var audit = await _dbContext.Audits
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == request.AuditId,
                cancellationToken);

        if (audit is null)
        {
            throw new InvalidOperationException(
                "La auditoría indicada no existe.");
        }

        await ValidateOwnerAsync(
            request.OwnerUserId,
            audit.OrganizationId,
            cancellationToken);

        var code = NormalizeCode(request.Code);

        var duplicate =
            await _dbContext.Risks.AnyAsync(
                x => x.AuditId == request.AuditId &&
                     x.Code == code,
                cancellationToken);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"Ya existe un riesgo con el código '{code}' en esta auditoría.");
        }

        var risk = new Risk(
            request.AuditId,
            code,
            request.Title,
            request.Description,
            request.Probability,
            request.Impact,
            request.Treatment,
            request.OwnerUserId);

        _dbContext.Risks.Add(risk);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return await GetByIdAsync(
                   risk.Id,
                   cancellationToken)
               ?? throw new InvalidOperationException(
                   "No fue posible recuperar el riesgo creado.");
    }

    public async Task<RiskDto?> UpdateAsync(
        Guid id,
        UpdateRiskRequest request,
        CancellationToken cancellationToken = default)
    {
        var risk = await _dbContext.Risks
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (risk is null)
        {
            return null;
        }

        var audit = await _dbContext.Audits
            .AsNoTracking()
            .SingleAsync(
                x => x.Id == risk.AuditId,
                cancellationToken);

        await ValidateOwnerAsync(
            request.OwnerUserId,
            audit.OrganizationId,
            cancellationToken);

        var code = NormalizeCode(request.Code);

        var duplicate =
            await _dbContext.Risks.AnyAsync(
                x => x.Id != id &&
                     x.AuditId == risk.AuditId &&
                     x.Code == code,
                cancellationToken);

        if (duplicate)
        {
            throw new InvalidOperationException(
                $"Ya existe otro riesgo con el código '{code}' en esta auditoría.");
        }

        risk.Update(
            code,
            request.Title,
            request.Description,
            request.Probability,
            request.Impact,
            request.Treatment,
            request.OwnerUserId);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return await GetByIdAsync(
            id,
            cancellationToken);
    }

    public Task<bool> StartTreatmentAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return ChangeStateAsync(
            id,
            risk => risk.StartTreatment(),
            cancellationToken);
    }

    public Task<bool> AcceptAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return ChangeStateAsync(
            id,
            risk => risk.Accept(),
            cancellationToken);
    }

    public Task<bool> MitigateAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return ChangeStateAsync(
            id,
            risk => risk.Mitigate(),
            cancellationToken);
    }

    public Task<bool> CloseAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return ChangeStateAsync(
            id,
            risk => risk.Close(),
            cancellationToken);
    }

    private async Task<bool> ChangeStateAsync(
        Guid id,
        Action<Risk> changeState,
        CancellationToken cancellationToken)
    {
        var risk = await _dbContext.Risks
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (risk is null)
        {
            return false;
        }

        changeState(risk);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    private async Task ValidateOwnerAsync(
        Guid? ownerUserId,
        Guid organizationId,
        CancellationToken cancellationToken)
    {
        if (!ownerUserId.HasValue)
        {
            return;
        }

        var owner = await _dbContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == ownerUserId.Value,
                cancellationToken);

        if (owner is null)
        {
            throw new InvalidOperationException(
                "El responsable del riesgo no existe.");
        }

        if (owner.OrganizationId != organizationId)
        {
            throw new InvalidOperationException(
                "El responsable del riesgo debe pertenecer a la organización de la auditoría.");
        }

        if (!owner.IsActive || owner.IsLocked)
        {
            throw new InvalidOperationException(
                "El responsable del riesgo debe estar activo y desbloqueado.");
        }
    }

    private static RiskLevel CalculateLevel(
        int score)
    {
        return score switch
        {
            <= 4 => RiskLevel.Low,
            <= 9 => RiskLevel.Medium,
            <= 16 => RiskLevel.High,
            _ => RiskLevel.Critical
        };
    }

    private static string NormalizeCode(
        string code)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);

        return code.Trim().ToUpperInvariant();
    }
}
