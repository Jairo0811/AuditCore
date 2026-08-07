using AuditCore.Application.Common.Security;
using AuditCore.Application.Features.ActionPlans;
using AuditCore.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditCore.Api.Controllers;

[ApiController]
[Route("api/action-plans")]
[Authorize]
public sealed class ActionPlansController : ControllerBase
{
    private readonly IActionPlanService _service;
    public ActionPlansController(IActionPlanService service) => _service = service;

    [HttpGet]
    [Authorize(Policy = PermissionCodes.ActionPlansView)]
    public async Task<ActionResult<IReadOnlyCollection<ActionPlanDto>>> GetAll([FromQuery] Guid? findingId, [FromQuery] ActionPlanStatus? status, CancellationToken cancellationToken) =>
        Ok(await _service.GetAllAsync(findingId, status, cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCodes.ActionPlansView)]
    public async Task<ActionResult<ActionPlanDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var entity = await _service.GetByIdAsync(id, cancellationToken);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.ActionPlansManage)]
    public async Task<ActionResult<ActionPlanDto>> Create(CreateActionPlanRequest request, CancellationToken cancellationToken)
    {
        var entity = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCodes.ActionPlansManage)]
    public async Task<ActionResult<ActionPlanDto>> Update(Guid id, UpdateActionPlanRequest request, CancellationToken cancellationToken)
    {
        var entity = await _service.UpdateAsync(id, request, cancellationToken);
        return entity is null ? NotFound() : Ok(entity);
    }

    [HttpPut("{id:guid}/progress")]
    [Authorize(Policy = PermissionCodes.ActionPlansManage)]
    public async Task<IActionResult> SetProgress(Guid id, SetActionPlanProgressRequest request, CancellationToken cancellationToken) =>
        await _service.SetProgressAsync(id, request.ProgressPercent, cancellationToken) ? NoContent() : NotFound();

    [HttpPut("{id:guid}/complete")]
    [Authorize(Policy = PermissionCodes.ActionPlansManage)]
    public async Task<IActionResult> Complete(Guid id, CompleteActionPlanRequest request, CancellationToken cancellationToken) =>
        await _service.CompleteAsync(id, request.Notes, cancellationToken) ? NoContent() : NotFound();

    [HttpPut("{id:guid}/cancel")]
    [Authorize(Policy = PermissionCodes.ActionPlansManage)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken) =>
        await _service.CancelAsync(id, cancellationToken) ? NoContent() : NotFound();

    [HttpPost("mark-overdue")]
    [Authorize(Policy = PermissionCodes.ActionPlansManage)]
    public async Task<ActionResult<int>> MarkOverdue(CancellationToken cancellationToken) =>
        Ok(await _service.MarkOverdueAsync(cancellationToken));
}
