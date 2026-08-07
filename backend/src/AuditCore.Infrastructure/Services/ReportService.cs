using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using AuditCore.Application.Common.Interfaces;
using AuditCore.Application.Features.Reports;
using AuditCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditCore.Infrastructure.Services;

public sealed class ReportService : IReportService
{
    private readonly IAuditCoreDbContext _dbContext;
    private readonly TenantGuard _tenantGuard;

    public ReportService(IAuditCoreDbContext dbContext, TenantGuard tenantGuard)
    {
        _dbContext = dbContext;
        _tenantGuard = tenantGuard;
    }

    public async Task<DashboardDto> GetDashboardAsync(Guid? organizationId = null, CancellationToken cancellationToken = default)
    {
        organizationId = ResolveOrganization(organizationId);
        var audits = _dbContext.Audits.AsNoTracking().AsQueryable();
        if (organizationId.HasValue) audits = audits.Where(x => x.OrganizationId == organizationId.Value);
        var auditIds = audits.Select(x => x.Id);

        var totalAudits = await audits.CountAsync(cancellationToken);
        var closedAudits = await audits.CountAsync(x => x.Status == AuditStatus.Closed, cancellationToken);
        var totalRisks = await _dbContext.Risks.CountAsync(x => auditIds.Contains(x.AuditId), cancellationToken);
        var criticalRisks = await _dbContext.Risks.CountAsync(x => auditIds.Contains(x.AuditId) && x.Probability * x.Impact >= 17, cancellationToken);
        var totalFindings = await _dbContext.Findings.CountAsync(x => auditIds.Contains(x.AuditId), cancellationToken);
        var openFindings = await _dbContext.Findings.CountAsync(x => auditIds.Contains(x.AuditId) && x.Status != FindingStatus.Closed, cancellationToken);
        var overduePlans = await _dbContext.ActionPlans.CountAsync(x => auditIds.Contains(x.Finding.AuditId) && x.Status == ActionPlanStatus.Overdue, cancellationToken);
        var score = await _dbContext.ControlEvaluations
            .Where(x => auditIds.Contains(x.AuditId) && x.Score.HasValue)
            .Select(x => (decimal?)x.Score!.Value)
            .AverageAsync(cancellationToken) ?? 0;

        return new DashboardDto(totalAudits, totalAudits - closedAudits, closedAudits, totalRisks, criticalRisks, totalFindings, openFindings, overduePlans, Math.Round(score, 2));
    }

    public async Task<ReportFile> ExportAuditSummaryAsync(Guid? organizationId, ReportFormat format, CancellationToken cancellationToken = default)
    {
        organizationId = ResolveOrganization(organizationId);
        var query = _dbContext.Audits.AsNoTracking().AsQueryable();
        if (organizationId.HasValue) query = query.Where(x => x.OrganizationId == organizationId.Value);

        var data = await query.OrderBy(x => x.Code)
            .Select(x => new { x.Code, x.Title, Organization = x.Organization.Name, x.Status, x.StartDateUtc, x.EndDateUtc })
            .ToListAsync(cancellationToken);
        var rows = data.Select(x => new[]
        {
            x.Code,
            x.Title,
            x.Organization,
            x.Status.ToString(),
            x.StartDateUtc?.ToString("yyyy-MM-dd") ?? string.Empty,
            x.EndDateUtc?.ToString("yyyy-MM-dd") ?? string.Empty
        }).ToArray();
        var headers = new[] { "Code", "Title", "Organization", "Status", "StartDate", "EndDate" };

        return format switch
        {
            ReportFormat.Csv => new ReportFile("auditcore-audits.csv", "text/csv", BuildCsv(headers, rows)),
            ReportFormat.Excel => new ReportFile("auditcore-audits.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", BuildXlsx(headers, rows)),
            ReportFormat.Pdf => new ReportFile("auditcore-audits.pdf", "application/pdf", BuildPdf(headers, rows)),
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };
    }

    private Guid? ResolveOrganization(Guid? requested)
    {
        var restricted = _tenantGuard.RestrictedOrganizationId;
        if (restricted.HasValue)
        {
            if (requested.HasValue && requested.Value != restricted.Value)
                throw new UnauthorizedAccessException("No tiene acceso a reportes de otra organización.");
            return restricted.Value;
        }
        return requested;
    }

    private static byte[] BuildCsv(string[] headers, IReadOnlyCollection<string[]> rows)
    {
        static string Esc(string v) => $"\"{v.Replace("\"", "\"\"")}\"";
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(',', headers.Select(Esc)));
        foreach (var row in rows) sb.AppendLine(string.Join(',', row.Select(Esc)));
        return Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();
    }

    private static byte[] BuildXlsx(string[] headers, IReadOnlyCollection<string[]> rows)
    {
        using var output = new MemoryStream();
        using (var zip = new ZipArchive(output, ZipArchiveMode.Create, true))
        {
            Write(zip, "[Content_Types].xml", "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Types xmlns=\"http://schemas.openxmlformats.org/package/2006/content-types\"><Default Extension=\"rels\" ContentType=\"application/vnd.openxmlformats-package.relationships+xml\"/><Default Extension=\"xml\" ContentType=\"application/xml\"/><Override PartName=\"/xl/workbook.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml\"/><Override PartName=\"/xl/worksheets/sheet1.xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/></Types>");
            Write(zip, "_rels/.rels", "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument\" Target=\"xl/workbook.xml\"/></Relationships>");
            Write(zip, "xl/workbook.xml", "<?xml version=\"1.0\" encoding=\"UTF-8\"?><workbook xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\" xmlns:r=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships\"><sheets><sheet name=\"Audits\" sheetId=\"1\" r:id=\"rId1\"/></sheets></workbook>");
            Write(zip, "xl/_rels/workbook.xml.rels", "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Relationships xmlns=\"http://schemas.openxmlformats.org/package/2006/relationships\"><Relationship Id=\"rId1\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet1.xml\"/></Relationships>");
            var allRows = new List<string[]> { headers };
            allRows.AddRange(rows);
            var sheet = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\"?><worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>");
            var rowNumber = 1;
            foreach (var row in allRows)
            {
                sheet.Append($"<row r=\"{rowNumber++}\">");
                foreach (var value in row) sheet.Append($"<c t=\"inlineStr\"><is><t>{Xml(value)}</t></is></c>");
                sheet.Append("</row>");
            }
            sheet.Append("</sheetData></worksheet>");
            Write(zip, "xl/worksheets/sheet1.xml", sheet.ToString());
        }
        return output.ToArray();
    }

    private static byte[] BuildPdf(string[] headers, IReadOnlyCollection<string[]> rows)
    {
        var lines = new List<string> { "AuditCore - Audit Summary", string.Join(" | ", headers) };
        lines.AddRange(rows.Take(35).Select(row => string.Join(" | ", row)));
        var content = new StringBuilder("BT /F1 8 Tf 35 800 Td 11 TL ");
        foreach (var line in lines) content.Append($"({PdfText(line)}) Tj T* ");
        content.Append("ET");
        var stream = content.ToString();
        var objects = new[]
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
            "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] /Resources << /Font << /F1 5 0 R >> >> /Contents 4 0 R >>",
            $"<< /Length {Encoding.ASCII.GetByteCount(stream)} >>\nstream\n{stream}\nendstream",
            "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica >>"
        };
        using var ms = new MemoryStream();
        void WriteAscii(string value) { var bytes = Encoding.ASCII.GetBytes(value); ms.Write(bytes); }
        WriteAscii("%PDF-1.4\n");
        var offsets = new List<long> { 0 };
        for (var i = 0; i < objects.Length; i++)
        {
            offsets.Add(ms.Position);
            WriteAscii($"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
        }
        var xref = ms.Position;
        WriteAscii($"xref\n0 {objects.Length + 1}\n0000000000 65535 f \n");
        for (var i = 1; i < offsets.Count; i++) WriteAscii($"{offsets[i]:0000000000} 00000 n \n");
        WriteAscii($"trailer << /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF");
        return ms.ToArray();
    }

    private static void Write(ZipArchive zip, string path, string text)
    {
        var entry = zip.CreateEntry(path, CompressionLevel.Fastest);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        writer.Write(text);
    }

    private static string Xml(string value) => new XText(value).ToString();

    private static string PdfText(string value) =>
        new string(value.Select(ch => ch is >= ' ' and <= '~' ? ch : '?').ToArray())
            .Replace("\\", "\\\\")
            .Replace("(", "\\(")
            .Replace(")", "\\)");
}
