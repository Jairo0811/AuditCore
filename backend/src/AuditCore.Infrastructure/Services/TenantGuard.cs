using AuditCore.Application.Common.Security;

namespace AuditCore.Infrastructure.Services;

public sealed class TenantGuard
{
    private readonly ICurrentUserContext _currentUser;
    public TenantGuard(ICurrentUserContext currentUser) => _currentUser = currentUser;

    public Guid? RestrictedOrganizationId =>
        _currentUser.IsAuthenticated && !_currentUser.IsSuperAdmin
            ? _currentUser.OrganizationId ?? throw new UnauthorizedAccessException("El token no contiene una organización válida.")
            : null;

    public void EnsureOrganization(Guid organizationId)
    {
        var restricted = RestrictedOrganizationId;
        if (restricted.HasValue && restricted.Value != organizationId)
            throw new UnauthorizedAccessException("No tiene acceso a recursos de otra organización.");
    }
}
