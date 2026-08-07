using AuditCore.Application.Common.Security;
using AuditCore.Application.Features.Branches;
using AuditCore.Application.Features.Branches.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditCore.Api.Controllers;

[ApiController]
[Route("api/branches")]
[Authorize]
public sealed class BranchesController
    : ControllerBase
{
    private readonly IBranchService _service;

    public BranchesController(
        IBranchService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.OrganizationsView)]
    public async Task<ActionResult<IReadOnlyCollection<BranchDto>>> GetAll(
        [FromQuery] Guid? organizationId,
        CancellationToken cancellationToken)
    {
        var branches =
            await _service.GetAllAsync(
                organizationId,
                cancellationToken);

        return Ok(branches);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCodes.OrganizationsView)]
    public async Task<ActionResult<BranchDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var branch =
            await _service.GetByIdAsync(
                id,
                cancellationToken);

        return branch is null
            ? NotFound()
            : Ok(branch);
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.OrganizationsManage)]
    public async Task<ActionResult<BranchDto>> Create(
        CreateBranchRequest request,
        CancellationToken cancellationToken)
    {
        var branch =
            await _service.CreateAsync(
                request,
                cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = branch.Id },
            branch);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCodes.OrganizationsManage)]
    public async Task<ActionResult<BranchDto>> Update(
        Guid id,
        UpdateBranchRequest request,
        CancellationToken cancellationToken)
    {
        var branch =
            await _service.UpdateAsync(
                id,
                request,
                cancellationToken);

        return branch is null
            ? NotFound()
            : Ok(branch);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionCodes.OrganizationsManage)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted =
            await _service.DeleteAsync(
                id,
                cancellationToken);

        return deleted
            ? NoContent()
            : NotFound();
    }
}