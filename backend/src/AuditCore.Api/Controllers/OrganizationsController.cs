using AuditCore.Application.Common.Security;
using AuditCore.Application.Features.Organizations;
using AuditCore.Application.Features.Organizations.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditCore.Api.Controllers;

[ApiController]
[Route("api/organizations")]
[Authorize]
public sealed class OrganizationsController
    : ControllerBase
{
    private readonly IOrganizationService _service;

    public OrganizationsController(
        IOrganizationService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.OrganizationsView)]
    public async Task<ActionResult<IReadOnlyCollection<OrganizationDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var organizations =
            await _service.GetAllAsync(
                cancellationToken);

        return Ok(organizations);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCodes.OrganizationsView)]
    public async Task<ActionResult<OrganizationDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var organization =
            await _service.GetByIdAsync(
                id,
                cancellationToken);

        return organization is null
            ? NotFound()
            : Ok(organization);
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.OrganizationsManage)]
    public async Task<ActionResult<OrganizationDto>> Create(
        CreateOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        var organization =
            await _service.CreateAsync(
                request,
                cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = organization.Id },
            organization);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCodes.OrganizationsManage)]
    public async Task<ActionResult<OrganizationDto>> Update(
        Guid id,
        UpdateOrganizationRequest request,
        CancellationToken cancellationToken)
    {
        var organization =
            await _service.UpdateAsync(
                id,
                request,
                cancellationToken);

        return organization is null
            ? NotFound()
            : Ok(organization);
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