using AuditCore.Application.Common.Security;
using AuditCore.Application.Features.Permissions;
using AuditCore.Application.Features.Permissions.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditCore.Api.Controllers;

[ApiController]
[Route("api/permissions")]
[Authorize]
public sealed class PermissionsController : ControllerBase
{
    private readonly IPermissionService _service;

    public PermissionsController(
        IPermissionService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.RolesView)]
    public async Task<ActionResult<IReadOnlyCollection<PermissionDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetAllAsync(
                cancellationToken));
    }
}
