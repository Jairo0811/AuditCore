using AuditCore.Application.Common.Security;
using AuditCore.Application.Features.Users;
using AuditCore.Application.Features.Users.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditCore.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _service;

    public UsersController(IUserService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.UsersView)]
    public async Task<ActionResult<IReadOnlyCollection<UserDto>>> GetAll(
        [FromQuery] Guid? organizationId,
        CancellationToken cancellationToken)
    {
        return Ok(await _service.GetAllAsync(
            organizationId,
            cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCodes.UsersView)]
    public async Task<ActionResult<UserDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var user = await _service.GetByIdAsync(id, cancellationToken);

        return user is null
            ? NotFound()
            : Ok(user);
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.UsersManage)]
    public async Task<ActionResult<UserDto>> Create(
        CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _service.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = user.Id },
            user);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCodes.UsersManage)]
    public async Task<ActionResult<UserDto>> Update(
        Guid id,
        UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var user = await _service.UpdateAsync(
            id,
            request,
            cancellationToken);

        return user is null
            ? NotFound()
            : Ok(user);
    }

    [HttpPut("{id:guid}/password")]
    [Authorize(Policy = PermissionCodes.UsersManage)]
    public async Task<IActionResult> ChangePassword(
        Guid id,
        ChangeUserPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.ChangePasswordAsync(
            id,
            request,
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpPut("{id:guid}/roles")]
    [Authorize(Policy = PermissionCodes.UsersManage)]
    public async Task<IActionResult> SetRoles(
        Guid id,
        SetUserRolesRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _service.SetRolesAsync(
            id,
            request,
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpPut("{id:guid}/activate")]
    [Authorize(Policy = PermissionCodes.UsersManage)]
    public async Task<IActionResult> Activate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var updated = await _service.SetActiveAsync(
            id,
            true,
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpPut("{id:guid}/deactivate")]
    [Authorize(Policy = PermissionCodes.UsersManage)]
    public async Task<IActionResult> Deactivate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var updated = await _service.SetActiveAsync(
            id,
            false,
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpPut("{id:guid}/lock")]
    [Authorize(Policy = PermissionCodes.UsersManage)]
    public async Task<IActionResult> Lock(
        Guid id,
        CancellationToken cancellationToken)
    {
        var updated = await _service.SetLockedAsync(
            id,
            true,
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpPut("{id:guid}/unlock")]
    [Authorize(Policy = PermissionCodes.UsersManage)]
    public async Task<IActionResult> Unlock(
        Guid id,
        CancellationToken cancellationToken)
    {
        var updated = await _service.SetLockedAsync(
            id,
            false,
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }
}
