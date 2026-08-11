using System.Globalization;
using System.Text;

namespace AuditCore.Infrastructure.Services.Reports;

internal static class AuditSummaryPdfBuilder
{
    private const double PageWidth = 842;
    private const double PageHeight = 595;
    private const double Margin = 40;
    private const double TableTop = 406;
    private const double RowHeight = 28;
    private const int RowsPerPage = 11;

    private static readonly ColumnDefinition[] Columns =
    [
        new("CÓDIGO", 78, 12),
        new("TÍTULO", 222, 34),
        new("ORGANIZACIÓN", 168, 25),
        new("ESTADO", 92, 14),
        new("INICIO", 80, 10),
        new("FIN", 80, 10)
    ];

    public static byte[] Build(IReadOnlyCollection<string[]> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        var rowList = rows.ToArray();
        var pageCount = Math.Max(1, (int)Math.Ceiling(rowList.Length / (double)RowsPerPage));
        var pageStreams = new List<string>(pageCount);

        for (var pageIndex = 0; pageIndex < pageCount; pageIndex++)
        {
            var pageRows = rowList
                .Skip(pageIndex * RowsPerPage)
                .Take(RowsPerPage)
                .ToArray();

            pageStreams.Add(BuildPageContent(pageRows, rowList.Length, pageIndex + 1, pageCount));
        }

        return BuildPdfDocument(pageStreams);
    }

    private static string BuildPageContent(
        IReadOnlyCollection<string[]> rows,
        int totalRows,
        int pageNumber,
        int pageCount)
    {
        var content = new StringBuilder();

        DrawRectangle(content, 0, 0, PageWidth, PageHeight, "0.024 0.067 0.122");
        DrawRectangle(content, 0, 470, PageWidth, 125, "0.027 0.094 0.165");
        DrawRectangle(content, 0, 468, PageWidth, 2, "0.078 0.722 0.651");

        DrawText(content, "AUDIT", 42, 548, 26, bold: true, color: "0.918 0.949 0.976");
        DrawText(content, "CORE", 126, 548, 26, bold: true, color: "0.078 0.722 0.651");
        DrawText(content, "ENTERPRISE IT AUDIT & COMPLIANCE PLATFORM", 42, 526, 8.5, bold: true, color: "0.557 0.647 0.741");

        DrawText(content, "Resumen de auditorías", 42, 488, 17, bold: true, color: "0.918 0.949 0.976");
        DrawText(content, "Reporte ejecutivo consolidado", 42, 472, 9, color: "0.663 0.741 0.816");

        var generatedAt = DateTimeOffset.Now.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);
        DrawText(content, $"Generado: {generatedAt}", 620, 548, 8.5, color: "0.663 0.741 0.816");

        DrawRoundedMetric(content, 620, 493, 180, 39, totalRows);

        if (totalRows == 0)
        {
            DrawEmptyState(content);
        }
        else
        {
            DrawTable(content, rows);
        }

        DrawLine(content, Margin, 31, PageWidth - Margin, 31, "0.122 0.224 0.329", 0.7);
        DrawText(content, "AuditCore · Auditoría, evaluación y cumplimiento TI", Margin, 17, 7.5, color: "0.459 0.553 0.647");
        DrawText(content, $"Página {pageNumber} de {pageCount}", 728, 17, 7.5, color: "0.459 0.553 0.647");

        return content.ToString();
    }

    private static void DrawRoundedMetric(StringBuilder content, double x, double y, double width, double height, int totalRows)
    {
        DrawRectangle(content, x, y, width, height, "0.039 0.125 0.212");
        DrawLine(content, x, y, x + width, y, "0.078 0.722 0.651", 1.3);
        DrawText(content, "TOTAL DE AUDITORÍAS", x + 12, y + 24, 7.5, bold: true, color: "0.557 0.647 0.741");
        DrawText(content, totalRows.ToString(CultureInfo.InvariantCulture), x + 12, y + 7, 16, bold: true, color: "0.345 0.918 0.831");
    }

    private static void DrawEmptyState(StringBuilder content)
    {
        DrawRectangle(content, Margin, 185, PageWidth - (Margin * 2), 180, "0.039 0.125 0.212");
        DrawRectangleStroke(content, Margin, 185, PageWidth - (Margin * 2), 180, "0.122 0.224 0.329", 1);
        DrawText(content, "SIN DATOS PARA MOSTRAR", 300, 284, 13, bold: true, color: "0.345 0.918 0.831");
        DrawText(content, "No hay auditorías disponibles para los filtros seleccionados.", 236, 255, 10, color: "0.663 0.741 0.816");
        DrawText(content, "El reporte se actualizará automáticamente cuando existan registros.", 220, 236, 9, color: "0.459 0.553 0.647");
    }

    private static void DrawTable(StringBuilder content, IReadOnlyCollection<string[]> rows)
    {
        var tableWidth = Columns.Sum(column => column.Width);
        var x = Margin;
        var headerBottom = TableTop - RowHeight;

        DrawRectangle(content, Margin, headerBottom, tableWidth, RowHeight, "0.039 0.157 0.255");
        DrawRectangleStroke(content, Margin, headerBottom, tableWidth, RowHeight, "0.078 0.722 0.651", 0.9);

        foreach (var column in Columns)
        {
            DrawText(content, column.Header, x + 8, headerBottom + 9, 7.5, bold: true, color: "0.800 0.984 0.945");
            x += column.Width;
        }

        var rowBottom = headerBottom - RowHeight;
        var rowIndex = 0;

        foreach (var row in rows)
        {
            var fill = rowIndex % 2 == 0 ? "0.031 0.098 0.169" : "0.035 0.114 0.192";
            DrawRectangle(content, Margin, rowBottom, tableWidth, RowHeight, fill);
            DrawRectangleStroke(content, Margin, rowBottom, tableWidth, RowHeight, "0.102 0.196 0.286", 0.45);

            x = Margin;
            for (var index = 0; index < Columns.Length; index++)
            {
                var value = index < row.Length ? row[index] : string.Empty;
                value = Truncate(value, Columns[index].MaxCharacters);
                var color = index == 0 ? "0.345 0.918 0.831" : "0.843 0.890 0.933";
                DrawText(content, value, x + 8, rowBottom + 9, 7.8, bold: index == 0, color: color);
                x += Columns[index].Width;
            }

            rowBottom -= RowHeight;
            rowIndex++;
        }
    }

    private static byte[] BuildPdfDocument(IReadOnlyList<string> pageStreams)
    {
        var pageCount = pageStreams.Count;
        var regularFontObjectId = 3 + (pageCount * 2);
        var boldFontObjectId = regularFontObjectId + 1;
        var pageObjectIds = Enumerable.Range(0, pageCount).Select(index => 3 + (index * 2)).ToArray();

        var objects = new List<string>
        {
            "<< /Type /Catalog /Pages 2 0 R >>",
            $"<< /Type /Pages /Kids [{string.Join(' ', pageObjectIds.Select(id => $"{id} 0 R"))}] /Count {pageCount} >>"
        };

        for (var index = 0; index < pageCount; index++)
        {
            var contentObjectId = pageObjectIds[index] + 1;
            var stream = pageStreams[index];

            objects.Add(
                $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {PageWidth:0} {PageHeight:0}] " +
                $"/Resources << /Font << /F1 {regularFontObjectId} 0 R /F2 {boldFontObjectId} 0 R >> >> " +
                $"/Contents {contentObjectId} 0 R >>");

            objects.Add($"<< /Length {Encoding.ASCII.GetByteCount(stream)} >>\nstream\n{stream}\nendstream");
        }

        objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>");
        objects.Add("<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>");

        using var output = new MemoryStream();

        static byte[] Ascii(string value) => Encoding.ASCII.GetBytes(value);

        void WriteAscii(string value)
        {
            var bytes = Ascii(value);
            output.Write(bytes, 0, bytes.Length);
        }

        WriteAscii("%PDF-1.4\n%AuditCore\n");

        var offsets = new List<long> { 0 };
        for (var index = 0; index < objects.Count; index++)
        {
            offsets.Add(output.Position);
            WriteAscii($"{index + 1} 0 obj\n{objects[index]}\nendobj\n");
        }

        var xrefPosition = output.Position;
        WriteAscii($"xref\n0 {objects.Count + 1}\n0000000000 65535 f \n");

        for (var index = 1; index < offsets.Count; index++)
        {
            WriteAscii($"{offsets[index]:0000000000} 00000 n \n");
        }

        WriteAscii($"trailer << /Size {objects.Count + 1} /Root 1 0 R >>\nstartxref\n{xrefPosition}\n%%EOF");
        return output.ToArray();
    }

    private static void DrawText(
        StringBuilder content,
        string value,
        double x,
        double y,
        double fontSize,
        bool bold = false,
        string color = "1 1 1")
    {
        content.Append(color).Append(" rg ");
        content.Append("BT /").Append(bold ? "F2" : "F1").Append(' ')
            .Append(fontSize.ToString("0.##", CultureInfo.InvariantCulture)).Append(" Tf ")
            .Append(x.ToString("0.##", CultureInfo.InvariantCulture)).Append(' ')
            .Append(y.ToString("0.##", CultureInfo.InvariantCulture)).Append(" Td (")
            .Append(EscapePdfText(value)).Append(") Tj ET\n");
    }

    private static void DrawRectangle(
        StringBuilder content,
        double x,
        double y,
        double width,
        double height,
        string color)
    {
        content.Append(color).Append(" rg ")
            .Append(Number(x)).Append(' ').Append(Number(y)).Append(' ')
            .Append(Number(width)).Append(' ').Append(Number(height)).Append(" re f\n");
    }

    private static void DrawRectangleStroke(
        StringBuilder content,
        double x,
        double y,
        double width,
        double height,
        string color,
        double lineWidth)
    {
        content.Append(color).Append(" RG ")
            .Append(Number(lineWidth)).Append(" w ")
            .Append(Number(x)).Append(' ').Append(Number(y)).Append(' ')
            .Append(Number(width)).Append(' ').Append(Number(height)).Append(" re S\n");
    }

    private static void DrawLine(
        StringBuilder content,
        double x1,
        double y1,
        double x2,
        double y2,
        string color,
        double lineWidth)
    {
        content.Append(color).Append(" RG ")
            .Append(Number(lineWidth)).Append(" w ")
            .Append(Number(x1)).Append(' ').Append(Number(y1)).Append(" m ")
            .Append(Number(x2)).Append(' ').Append(Number(y2)).Append(" l S\n");
    }

    private static string EscapePdfText(string value)
    {
        var normalized = value
            .Replace('–', '-')
            .Replace('—', '-')
            .Replace('“', '"')
            .Replace('”', '"')
            .Replace('’', '\'');

        var bytes = Encoding.Latin1.GetBytes(normalized);
        var result = new StringBuilder(bytes.Length);

        foreach (var valueByte in bytes)
        {
            switch (valueByte)
            {
                case (byte)'\\':
                    result.Append("\\\\");
                    break;
                case (byte)'(':
                    result.Append("\\(");
                    break;
                case (byte)')':
                    result.Append("\\)");
                    break;
                default:
                    if (valueByte is < 32 or > 126)
                    {
                        result.Append('\\').Append(Convert.ToString(valueByte, 8)!.PadLeft(3, '0'));
                    }
                    else
                    {
                        result.Append((char)valueByte);
                    }
                    break;
            }
        }

        return result.ToString();
    }

    private static string Truncate(string value, int maxCharacters)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "-";
        }

        var trimmed = value.Trim();
        return trimmed.Length <= maxCharacters
            ? trimmed
            : $"{trimmed[..Math.Max(1, maxCharacters - 3)]}...";
    }

    private static string Number(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    private sealed record ColumnDefinition(string Header, double Width, int MaxCharacters);
}
