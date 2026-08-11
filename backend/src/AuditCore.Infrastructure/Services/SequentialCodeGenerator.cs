using AuditCore.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AuditCore.Infrastructure.Services;

internal sealed class SequentialCodeGenerator
{
    private readonly IAuditCoreDbContext _dbContext;

    public SequentialCodeGenerator(IAuditCoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<string> NextAuditCodeAsync(Guid organizationId, CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"AUD-{year}-";
        var existing = await _dbContext.Audits
            .AsNoTracking()
            .Where(x => x.OrganizationId == organizationId && x.Code.StartsWith(prefix))
            .Select(x => x.Code)
            .ToListAsync(cancellationToken);

        return Next(prefix, existing);
    }

    public async Task<string> NextRiskCodeAsync(Guid auditId, CancellationToken cancellationToken)
    {
        const string prefix = "RSK-";
        var existing = await _dbContext.Risks
            .AsNoTracking()
            .Where(x => x.AuditId == auditId && x.Code.StartsWith(prefix))
            .Select(x => x.Code)
            .ToListAsync(cancellationToken);

        return Next(prefix, existing);
    }

    public async Task<string> NextFindingCodeAsync(Guid auditId, CancellationToken cancellationToken)
    {
        const string prefix = "FND-";
        var existing = await _dbContext.Findings
            .AsNoTracking()
            .Where(x => x.AuditId == auditId && x.Code.StartsWith(prefix))
            .Select(x => x.Code)
            .ToListAsync(cancellationToken);

        return Next(prefix, existing);
    }

    private static string Next(string prefix, IEnumerable<string> existingCodes)
    {
        var max = existingCodes
            .Select(code => code[prefix.Length..])
            .Select(value => int.TryParse(value, out var number) ? number : 0)
            .DefaultIfEmpty(0)
            .Max();

        return $"{prefix}{max + 1:000}";
    }
}
