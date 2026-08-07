using AuditCore.Application.Common.Security;
using AuditCore.Application.Features.Findings;
using AuditCore.Application.Features.Findings.Models;
using AuditCore.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditCore.Api.Controllers;

[ApiController]
[Route("api/findings")]
[Authorize]
public sealed class FindingsController : ControllerBase
{
    private readonly IFindingService _service;

    public FindingsController(
        IFindingService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Policy = PermissionCodes.FindingsView)]
    public async Task<ActionResult<IReadOnlyCollection<FindingDto>>> GetAll(
        [FromQuery] Guid? auditId,
        [FromQuery] FindingStatus? status,
        [FromQuery] FindingSeverity? severity,
        CancellationToken cancellationToken)
    {
        return Ok(
            await _service.GetAllAsync(
                auditId,
                status,
                severity,
                cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCodes.FindingsView)]
    public async Task<ActionResult<FindingDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var finding =
            await _service.GetByIdAsync(
                id,
                cancellationToken);

        return finding is null
            ? NotFound()
            : Ok(finding);
    }

    [HttpPost]
    [Authorize(Policy = PermissionCodes.FindingsManage)]
    public async Task<ActionResult<FindingDto>> Create(
        CreateFindingRequest request,
        CancellationToken cancellationToken)
    {
        var finding =
            await _service.CreateAsync(
                request,
                cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = finding.Id },
            finding);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PermissionCodes.FindingsManage)]
    public async Task<ActionResult<FindingDto>> Update(
        Guid id,
        UpdateFindingRequest request,
        CancellationToken cancellationToken)
    {
        var finding =
            await _service.UpdateAsync(
                id,
                request,
                cancellationToken);

        return finding is null
            ? NotFound()
            : Ok(finding);
    }

    [HttpPut("{id:guid}/review")]
    [Authorize(Policy = PermissionCodes.FindingsManage)]
    public async Task<IActionResult> SendToReview(
        Guid id,
        CancellationToken cancellationToken)
    {
        var updated =
            await _service.SendToReviewAsync(
                id,
                cancellationToken);

        return updated
            ? NoContent()
            : NotFound();
    }

    [HttpPut("{id:guid}/accept")]
    [Authorize(Policy = PermissionCodes.FindingsManage)]
    public async Task<IActionResult> Accept(
        Guid id,
        CancellationToken cancellationToken)
    {
        var updated =
            await _service.AcceptAsync(
                id,
                cancellationToken);

        return updated
            ? NoContent()
            : NotFound();
    }

    [HttpPut("{id:guid}/resolve")]
    [Authorize(Policy = PermissionCodes.FindingsManage)]
    public async Task<IActionResult> Resolve(
        Guid id,
        CancellationToken cancellationToken)
    {
        var updated =
            await _service.ResolveAsync(
                id,
                cancellationToken);

        return updated
            ? NoContent()
            : NotFound();
    }

    [HttpPut("{id:guid}/close")]
    [Authorize(Policy = PermissionCodes.FindingsManage)]
    public async Task<IActionResult> Close(
        Guid id,
        CancellationToken cancellationToken)
    {
        var updated =
            await _service.CloseAsync(
                id,
                cancellationToken);

        return updated
            ? NoContent()
            : NotFound();
    }
}
