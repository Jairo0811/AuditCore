namespace AuditCore.Application.Features.Reports;

public sealed record DashboardDto(
    int TotalAudits,
    int OpenAudits,
    int ClosedAudits,
    int TotalRisks,
    int CriticalRisks,
    int TotalFindings,
    int OpenFindings,
    int OverdueActionPlans,
    decimal AverageComplianceScore);

public enum ReportFormat
{
    Csv = 1,
    Excel = 2,
    Pdf = 3
}

public sealed record ReportFile(string FileName, string ContentType, byte[] Content);

public interface IReportService
{
    Task<DashboardDto> GetDashboardAsync(Guid? organizationId = null, CancellationToken cancellationToken = default);
    Task<ReportFile> ExportAuditSummaryAsync(Guid? organizationId, ReportFormat format, CancellationToken cancellationToken = default);
}
