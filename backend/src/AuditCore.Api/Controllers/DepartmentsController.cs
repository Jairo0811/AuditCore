using AuditCore.Application.Common.Security;
using AuditCore.Application.Features.Departments;
using AuditCore.Application.Features.Departments.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditCore.Api.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize]
public sealed class DepartmentsController
    : ControllerBase
{
    private readonly IDepartmentService _service;

    public DepartmentsController(
        IDepartmentService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.OrganizationsView)]
    public async Task<ActionResult<IReadOnlyCollection<DepartmentDto>>> GetAll(
        [FromQuery] Guid? organizationId,
        [FromQuery] Guid? branchId,
        CancellationToken cancellationToken)
    {
        var departments =
            await _service.GetAllAsync(
                organizationId,
                branchId,
                cancellationToken);

        return Ok(departments);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCodes.OrganizationsView)]
    public async Task<ActionResult<DepartmentDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var department =
            await _service.GetByIdAsync(
                id,
                cancellationToken);

        return department is null
            ? NotFound()
            : Ok(department);
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.OrganizationsManage)]
    public async Task<ActionResult<DepartmentDto>> Create(
        CreateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var department =
            await _service.CreateAsync(
                request,
                cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = department.Id },
            department);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCodes.OrganizationsManage)]
    public async Task<ActionResult<DepartmentDto>> Update(
        Guid id,
        UpdateDepartmentRequest request,
        CancellationToken cancellationToken)
    {
        var department =
            await _service.UpdateAsync(
                id,
                request,
                cancellationToken);

        return department is null
            ? NotFound()
            : Ok(department);
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