using AuditCore.Application.Common.Security;
using AuditCore.Application.Features.Roles;
using AuditCore.Application.Features.Roles.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditCore.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize]
public sealed class RolesController : ControllerBase
{
    private readonly IRoleService _service;

    public RolesController(IRoleService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.RolesView)]
    public async Task<ActionResult<IReadOnlyCollection<RoleDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetAllAsync(cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCodes.RolesView)]
    public async Task<ActionResult<RoleDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var role = await _service.GetByIdAsync(
            id,
            cancellationToken);

        return role is null
            ? NotFound()
            : Ok(role);
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.RolesManage)]
    public async Task<ActionResult<RoleDto>> Create(
        CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var role = await _service.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = role.Id },
            role);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCodes.RolesManage)]
    public async Task<ActionResult<RoleDto>> Update(
        Guid id,
        UpdateRoleRequest request,
        CancellationToken cancellationToken)
    {
        var role = await _service.UpdateAsync(
            id,
            request,
            cancellationToken);

        return role is null
            ? NotFound()
            : Ok(role);
    }

    [HttpPut("{id:guid}/permissions")]
    [Authorize(Policy = PermissionCodes.RolesManage)]
    public async Task<IActionResult> SetPermissions(
        Guid id,
        SetRolePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.SetPermissionsAsync(
            id,
            request,
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }
}
