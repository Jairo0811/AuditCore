using AuditCore.Application.Common.Security;
using AuditCore.Application.Features.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuditCore.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public sealed class ReportsController : ControllerBase
{
    private readonly IReportService _service;
    public ReportsController(IReportService service) => _service = service;

    [HttpGet("dashboard")]
    [Authorize(Policy = PermissionCodes.ReportsView)]
    public async Task<ActionResult<DashboardDto>> Dashboard([FromQuery] Guid? organizationId, CancellationToken cancellationToken) =>
        Ok(await _service.GetDashboardAsync(organizationId, cancellationToken));

    [HttpGet("audits/export")]
    [Authorize(Policy = PermissionCodes.ReportsExport)]
    public async Task<IActionResult> ExportAudits([FromQuery] Guid? organizationId, [FromQuery] ReportFormat format = ReportFormat.Excel, CancellationToken cancellationToken = default)
    {
        var report = await _service.ExportAuditSummaryAsync(organizationId, format, cancellationToken);
        return File(report.Content, report.ContentType, report.FileName);
    }
}
