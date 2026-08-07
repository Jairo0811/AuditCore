using AuditCore.Application.Common.Security;
using AuditCore.Application.Features.Evidence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditCore.Api.Controllers;

[ApiController]
[Route("api/evidence")]
[Authorize]
public sealed class EvidenceController : ControllerBase
{
    private readonly IEvidenceService _service;
    public EvidenceController(IEvidenceService service) => _service = service;

    [HttpGet]
    [Authorize(Policy = PermissionCodes.EvidenceView)]
    public async Task<ActionResult<IReadOnlyCollection<EvidenceDto>>> GetAll([FromQuery] Guid? auditId, [FromQuery] Guid? findingId, CancellationToken cancellationToken) =>
        Ok(await _service.GetAllAsync(auditId, findingId, cancellationToken));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PermissionCodes.EvidenceView)]
    public async Task<ActionResult<EvidenceDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var evidence = await _service.GetByIdAsync(id, cancellationToken);
        return evidence is null ? NotFound() : Ok(evidence);
    }

    [HttpGet("{id:guid}/download")]
    [Authorize(Policy = PermissionCodes.EvidenceView)]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var file = await _service.DownloadAsync(id, cancellationToken);
        return file is null ? NotFound() : File(file.Value.Content, file.Value.Metadata.ContentType, file.Value.Metadata.FileName);
    }

    [HttpPost]
    [RequestSizeLimit(20 * 1024 * 1024)]
    [Authorize(Policy = PermissionCodes.EvidenceManage)]
    public async Task<ActionResult<EvidenceDto>> Create(
        [FromForm] Guid auditId,
        [FromForm] Guid? findingId,
        [FromForm] string? description,
        [FromForm] Guid? uploadedByUserId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file.Length <= 0) return BadRequest("El archivo está vacío.");
        await using var stream = file.OpenReadStream();
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory, cancellationToken);
        var result = await _service.CreateAsync(new CreateEvidenceRequest(auditId, findingId, file.FileName, file.ContentType, memory.ToArray(), description, uploadedByUserId), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PermissionCodes.EvidenceManage)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        await _service.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
}
