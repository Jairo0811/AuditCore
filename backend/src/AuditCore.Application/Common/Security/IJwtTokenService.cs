using AuditCore.Domain.Entities;

namespace AuditCore.Application.Common.Security;

public interface IJwtTokenService
{
    string GenerateAccessToken(
        User user,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions);
}
