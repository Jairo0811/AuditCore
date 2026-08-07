namespace AuditCore.Application.Common.Security;

public interface ICurrentUserContext
{
    bool IsAuthenticated { get; }
    Guid? UserId { get; }
    Guid? OrganizationId { get; }
    bool IsSuperAdmin { get; }
}
