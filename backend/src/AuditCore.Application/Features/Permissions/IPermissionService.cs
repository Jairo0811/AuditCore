using AuditCore.Application.Features.Permissions.Models;

namespace AuditCore.Application.Features.Permissions;

public interface IPermissionService
{
    Task<IReadOnlyCollection<PermissionDto>> GetAllAsync(
        CancellationToken cancellationToken = default);
}
