using AuditCore.Application.Common.Security;
using AuditCore.Application.Features.Audits;
using AuditCore.Application.Features.Audits.Models;
using AuditCore.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditCore.Api.Controllers;

[ApiController]
[Route("api/audits")]
[Authorize]
public sealed class AuditsController : ControllerBase
{
    private readonly IAuditService _service;

    public AuditsController(
        IAuditService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.AuditsView)]
    public async Task<ActionResult<IReadOnlyCollection<AuditDto>>> GetAll(
        [FromQuery] Guid? organizationId,
        [FromQuery] AuditStatus? status,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetAllAsync(
                organizationId,
                status,
                cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCodes.AuditsView)]
    public async Task<ActionResult<AuditDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var audit =
            await _service.GetByIdAsync(
                id,
                cancellationToken);

        return audit is null
            ? NotFound()
            : Ok(audit);
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.AuditsCreate)]
    public async Task<ActionResult<AuditDto>> Create(
        CreateAuditRequest request,
        CancellationToken cancellationToken)
    {
        var audit =
            await _service.CreateAsync(
                request,
                cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = audit.Id },
            audit);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCodes.AuditsUpdate)]
    public async Task<ActionResult<AuditDto>> Update(
        Guid id,
        UpdateAuditRequest request,
        CancellationToken cancellationToken)
    {
        var audit =
            await _service.UpdateAsync(
                id,
                request,
                cancellationToken);

        return audit is null
            ? NotFound()
            : Ok(audit);
    }

    [HttpPut("{id:guid}/plan")]
    [Authorize(Policy = PermissionCodes.AuditsUpdate)]
    public async Task<ActionResult<AuditDto>> Plan(
        Guid id,
        PlanAuditRequest request,
        CancellationToken cancellationToken)
    {
        var audit =
            await _service.PlanAsync(
                id,
                request,
                cancellationToken);

        return audit is null
            ? NotFound()
            : Ok(audit);
    }

    [HttpPut("{id:guid}/start")]
    [Authorize(Policy = PermissionCodes.AuditsExecute)]
    public async Task<IActionResult> Start(
        Guid id,
        CancellationToken cancellationToken)
    {
        var updated =
            await _service.StartAsync(
                id,
                cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpPut("{id:guid}/complete")]
    [Authorize(Policy = PermissionCodes.AuditsExecute)]
    public async Task<IActionResult> Complete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var updated =
            await _service.CompleteAsync(
                id,
                cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpPut("{id:guid}/close")]
    [Authorize(Policy = PermissionCodes.AuditsClose)]
    public async Task<IActionResult> Close(
        Guid id,
        CancellationToken cancellationToken)
    {
        var updated =
            await _service.CloseAsync(
                id,
                cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpPut("{id:guid}/cancel")]
    [Authorize(Policy = PermissionCodes.AuditsUpdate)]
    public async Task<IActionResult> Cancel(
        Guid id,
        CancellationToken cancellationToken)
    {
        var updated =
            await _service.CancelAsync(
                id,
                cancellationToken);

        return updated ? NoContent() : NotFound();
    }
}
