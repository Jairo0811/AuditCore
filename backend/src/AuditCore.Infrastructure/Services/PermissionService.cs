using AuditCore.Application.Common.Interfaces;
using AuditCore.Application.Features.Permissions;
using AuditCore.Application.Features.Permissions.Models;
using Microsoft.EntityFrameworkCore;

namespace AuditCore.Infrastructure.Services;

public sealed class PermissionService
    : IPermissionService
{
    private readonly IAuditCoreDbContext _dbContext;

    public PermissionService(
        IAuditCoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyCollection<PermissionDto>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Permissions
            .AsNoTracking()
            .OrderBy(x => x.Code)
            .Select(x => new PermissionDto(
                x.Id,
                x.Name,
                x.Code,
                x.Description,
                x.IsActive))
            .ToListAsync(cancellationToken);
    }
}
