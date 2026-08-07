using AuditCore.Application.Common.Security;
using AuditCore.Application.Features.Risks;
using AuditCore.Application.Features.Risks.Models;
using AuditCore.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditCore.Api.Controllers;

[ApiController]
[Route("api/risks")]
[Authorize]
public sealed class RisksController : ControllerBase
{
    private readonly IRiskService _service;

    public RisksController(
        IRiskService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.RisksView)]
    public async Task<ActionResult<IReadOnlyCollection<RiskDto>>> GetAll(
        [FromQuery] Guid? auditId,
        [FromQuery] RiskStatus? status,
        [FromQuery] RiskLevel? level,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetAllAsync(
                auditId,
                status,
                level,
                cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCodes.RisksView)]
    public async Task<ActionResult<RiskDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var risk = await _service.GetByIdAsync(
            id,
            cancellationToken);

        return risk is null
            ? NotFound()
            : Ok(risk);
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.RisksManage)]
    public async Task<ActionResult<RiskDto>> Create(
        CreateRiskRequest request,
        CancellationToken cancellationToken)
    {
        var risk = await _service.CreateAsync(
            request,
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = risk.Id },
            risk);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCodes.RisksManage)]
    public async Task<ActionResult<RiskDto>> Update(
        Guid id,
        UpdateRiskRequest request,
        CancellationToken cancellationToken)
    {
        var risk = await _service.UpdateAsync(
            id,
            request,
            cancellationToken);

        return risk is null
            ? NotFound()
            : Ok(risk);
    }

    [HttpPut("{id:guid}/start-treatment")]
    [Authorize(Policy = PermissionCodes.RisksManage)]
    public async Task<IActionResult> StartTreatment(
        Guid id,
        CancellationToken cancellationToken)
    {
        var updated = await _service.StartTreatmentAsync(
            id,
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpPut("{id:guid}/accept")]
    [Authorize(Policy = PermissionCodes.RisksManage)]
    public async Task<IActionResult> Accept(
        Guid id,
        CancellationToken cancellationToken)
    {
        var updated = await _service.AcceptAsync(
            id,
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpPut("{id:guid}/mitigate")]
    [Authorize(Policy = PermissionCodes.RisksManage)]
    public async Task<IActionResult> Mitigate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var updated = await _service.MitigateAsync(
            id,
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }

    [HttpPut("{id:guid}/close")]
    [Authorize(Policy = PermissionCodes.RisksManage)]
    public async Task<IActionResult> Close(
        Guid id,
        CancellationToken cancellationToken)
    {
        var updated = await _service.CloseAsync(
            id,
            cancellationToken);

        return updated ? NoContent() : NotFound();
    }
}
