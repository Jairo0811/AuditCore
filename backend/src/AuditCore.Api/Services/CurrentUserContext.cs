using System.Security.Claims;
using AuditCore.Application.Common.Security;

namespace AuditCore.Api.Services;

public sealed class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public CurrentUserContext(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
    public Guid? UserId => Parse(User?.FindFirstValue(ClaimTypes.NameIdentifier));
    public Guid? OrganizationId => Parse(User?.FindFirstValue("organization_id"));
    public bool IsSuperAdmin => User?.IsInRole("SUPER_ADMIN") == true;

    private static Guid? Parse(string? value) => Guid.TryParse(value, out var id) ? id : null;
}
