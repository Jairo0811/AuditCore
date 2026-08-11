using System.Buffers.Binary;
using System.Globalization;
using System.IO.Compression;
using System.Text;

namespace AuditCore.Infrastructure.Services.Reports;

internal static class AuditSummaryPdfBuilder
{
    private const double PageWidth = 842;
    private const double PageHeight = 595;

    private const double Margin = 40;

    private const double HeaderHeight = 156;
    private const double HeaderBottom = PageHeight - HeaderHeight;
    private const double HeaderDividerY = HeaderBottom - 2;

    private const double LogoX = 42;
    private const double LogoY = 500;
    private const double LogoMaxWidth = 340;
    private const double LogoMaxHeight = 92;

    private const double TitleX = 42;
    private const double TitleY = 468;
    private const double SubtitleY = 451;

    private const double RightPanelX = 615;
    private const double GeneratedAtY = 548;

    private const double MetricY = 492;
    private const double MetricWidth = 185;
    private const double MetricHeight = 42;

    private const double TableTop = 390;
    private const double RowHeight = 28;
    private const int RowsPerPage = 10;

    private const double EmptyStateY = 182;
    private const double EmptyStateHeight = 170;

    private static readonly ColumnDefinition[] Columns =
    [
        new("CÓDIGO", 78, 12),
        new("TÍTULO", 222, 34),
        new("ORGANIZACIÓN", 168, 25),
        new("ESTADO", 92, 14),
        new("INICIO", 80, 10),
        new("FIN", 80, 10)
    ];

    public static byte[] Build(
        IReadOnlyCollection<string[]> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);

        var logo = LoadLogo();

        var logoScale = Math.Min(
            LogoMaxWidth / logo.Width,
            LogoMaxHeight / logo.Height);

        var logoWidth = logo.Width * logoScale;
        var logoHeight = logo.Height * logoScale;

        var rowList = rows.ToArray();

        var pageCount = Math.Max(
            1,
            (int)Math.Ceiling(
                rowList.Length / (double)RowsPerPage));

        var pageStreams = new List<string>(pageCount);

        for (var pageIndex = 0;
             pageIndex < pageCount;
             pageIndex++)
        {
            var pageRows = rowList
                .Skip(pageIndex * RowsPerPage)
                .Take(RowsPerPage)
                .ToArray();

            pageStreams.Add(
                BuildPageContent(
                    pageRows,
                    rowList.Length,
                    pageIndex + 1,
                    pageCount,
                    logoWidth,
                    logoHeight));
        }

        return BuildPdfDocument(
            pageStreams,
            logo);
    }

    private static PngImageData LoadLogo()
    {
        var logoPath = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "Brand",
            "auditcore-logo.png");

        if (!File.Exists(logoPath))
        {
            throw new FileNotFoundException(
                "No se encontró el logo oficial de AuditCore " +
                $"en '{logoPath}'.",
                logoPath);
        }

        return PngImageData.Load(logoPath);
    }

    private static string BuildPageContent(
        IReadOnlyCollection<string[]> rows,
        int totalRows,
        int pageNumber,
        int pageCount,
        double logoWidth,
        double logoHeight)
    {
        var content = new StringBuilder();

        DrawRectangle(
            content,
            0,
            0,
            PageWidth,
            PageHeight,
            "0.024 0.067 0.122");

        DrawRectangle(
            content,
            0,
            HeaderBottom,
            PageWidth,
            HeaderHeight,
            "0.027 0.094 0.165");

        DrawRectangle(
            content,
            0,
            HeaderDividerY,
            PageWidth,
            2,
            "0.078 0.722 0.651");

        DrawImage(
            content,
            "ImLogo",
            LogoX,
            LogoY,
            logoWidth,
            logoHeight);

        DrawText(
            content,
            "Resumen de auditorías",
            TitleX,
            TitleY,
            18,
            bold: true,
            color: "0.918 0.949 0.976");

        DrawText(
            content,
            "Reporte ejecutivo consolidado",
            TitleX,
            SubtitleY,
            9.2,
            color: "0.663 0.741 0.816");

        var generatedAt = DateTimeOffset.Now.ToString(
            "dd/MM/yyyy HH:mm",
            CultureInfo.InvariantCulture);

        DrawText(
            content,
            $"Generado: {generatedAt}",
            RightPanelX,
            GeneratedAtY,
            8.5,
            color: "0.663 0.741 0.816");

        DrawMetric(
            content,
            RightPanelX,
            MetricY,
            MetricWidth,
            MetricHeight,
            totalRows);

        if (totalRows == 0)
        {
            DrawEmptyState(content);
        }
        else
        {
            DrawTable(content, rows);
        }

        DrawFooter(
            content,
            pageNumber,
            pageCount);

        return content.ToString();
    }

    private static void DrawMetric(
        StringBuilder content,
        double x,
        double y,
        double width,
        double height,
        int totalRows)
    {
        DrawRectangle(
            content,
            x,
            y,
            width,
            height,
            "0.039 0.125 0.212");

        DrawRectangleStroke(
            content,
            x,
            y,
            width,
            height,
            "0.102 0.196 0.286",
            0.7);

        DrawLine(
            content,
            x,
            y,
            x + width,
            y,
            "0.078 0.722 0.651",
            1.5);

        DrawText(
            content,
            "TOTAL DE AUDITORÍAS",
            x + 12,
            y + 26,
            7.6,
            bold: true,
            color: "0.557 0.647 0.741");

        DrawText(
            content,
            totalRows.ToString(
                CultureInfo.InvariantCulture),
            x + 12,
            y + 7,
            17,
            bold: true,
            color: "0.345 0.918 0.831");
    }

    private static void DrawEmptyState(
        StringBuilder content)
    {
        var width = PageWidth - (Margin * 2);

        DrawRectangle(
            content,
            Margin,
            EmptyStateY,
            width,
            EmptyStateHeight,
            "0.039 0.125 0.212");

        DrawRectangleStroke(
            content,
            Margin,
            EmptyStateY,
            width,
            EmptyStateHeight,
            "0.122 0.224 0.329",
            1);

        DrawLine(
            content,
            Margin + 230,
            EmptyStateY + 117,
            PageWidth - Margin - 230,
            EmptyStateY + 117,
            "0.078 0.722 0.651",
            1);

        DrawText(
            content,
            "SIN DATOS PARA MOSTRAR",
            300,
            EmptyStateY + 96,
            13.5,
            bold: true,
            color: "0.345 0.918 0.831");

        DrawText(
            content,
            "No hay auditorías disponibles para los filtros seleccionados.",
            236,
            EmptyStateY + 67,
            10,
            color: "0.663 0.741 0.816");

        DrawText(
            content,
            "El reporte se actualizará automáticamente cuando existan registros.",
            220,
            EmptyStateY + 47,
            9,
            color: "0.459 0.553 0.647");
    }

    private static void DrawTable(
        StringBuilder content,
        IReadOnlyCollection<string[]> rows)
    {
        var tableWidth = Columns.Sum(
            column => column.Width);

        var x = Margin;
        var headerBottom = TableTop - RowHeight;

        DrawRectangle(
            content,
            Margin,
            headerBottom,
            tableWidth,
            RowHeight,
            "0.039 0.157 0.255");

        DrawRectangleStroke(
            content,
            Margin,
            headerBottom,
            tableWidth,
            RowHeight,
            "0.078 0.722 0.651",
            0.9);

        foreach (var column in Columns)
        {
            DrawText(
                content,
                column.Header,
                x + 8,
                headerBottom + 9,
                7.5,
                bold: true,
                color: "0.800 0.984 0.945");

            x += column.Width;
        }

        var rowBottom = headerBottom - RowHeight;
        var rowIndex = 0;

        foreach (var row in rows)
        {
            var fill = rowIndex % 2 == 0
                ? "0.031 0.098 0.169"
                : "0.035 0.114 0.192";

            DrawRectangle(
                content,
                Margin,
                rowBottom,
                tableWidth,
                RowHeight,
                fill);

            DrawRectangleStroke(
                content,
                Margin,
                rowBottom,
                tableWidth,
                RowHeight,
                "0.102 0.196 0.286",
                0.45);

            x = Margin;

            for (var index = 0;
                 index < Columns.Length;
                 index++)
            {
                var value = index < row.Length
                    ? row[index]
                    : string.Empty;

                value = Truncate(
                    value,
                    Columns[index].MaxCharacters);

                var color = index == 0
                    ? "0.345 0.918 0.831"
                    : "0.843 0.890 0.933";

                DrawText(
                    content,
                    value,
                    x + 8,
                    rowBottom + 9,
                    7.8,
                    bold: index == 0,
                    color: color);

                x += Columns[index].Width;
            }

            rowBottom -= RowHeight;
            rowIndex++;
        }
    }

    private static void DrawFooter(
        StringBuilder content,
        int pageNumber,
        int pageCount)
    {
        DrawLine(
            content,
            Margin,
            31,
            PageWidth - Margin,
            31,
            "0.122 0.224 0.329",
            0.7);

        DrawText(
            content,
            "AuditCore - Auditoría, evaluación y cumplimiento TI",
            Margin,
            17,
            7.5,
            color: "0.459 0.553 0.647");

        DrawText(
            content,
            $"Página {pageNumber} de {pageCount}",
            728,
            17,
            7.5,
            color: "0.459 0.553 0.647");
    }

    private static byte[] BuildPdfDocument(
        IReadOnlyList<string> pageStreams,
        PngImageData logo)
    {
        var pageCount = pageStreams.Count;

        var regularFontObjectId =
            3 + (pageCount * 2);

        var boldFontObjectId =
            regularFontObjectId + 1;

        var logoObjectId =
            boldFontObjectId + 1;

        var alphaObjectId = logo.HasAlpha
            ? logoObjectId + 1
            : (int?)null;

        var pageObjectIds = Enumerable
            .Range(0, pageCount)
            .Select(index => 3 + (index * 2))
            .ToArray();

        var objects = new List<PdfObject>();

        objects.Add(
            PdfObject.FromText(
                "<< /Type /Catalog /Pages 2 0 R >>"));

        var pageReferences = string.Join(
            " ",
            pageObjectIds.Select(
                id => $"{id} 0 R"));

        objects.Add(
            PdfObject.FromText(
                $"<< /Type /Pages /Kids [{pageReferences}] " +
                $"/Count {pageCount} >>"));

        for (var index = 0;
             index < pageCount;
             index++)
        {
            var contentObjectId =
                pageObjectIds[index] + 1;

            var pageResources =
                $"<< /Font << " +
                $"/F1 {regularFontObjectId} 0 R " +
                $"/F2 {boldFontObjectId} 0 R >> " +
                $"/XObject << /ImLogo {logoObjectId} 0 R >> >>";

            objects.Add(
                PdfObject.FromText(
                    $"<< /Type /Page " +
                    $"/Parent 2 0 R " +
                    $"/MediaBox [0 0 {PageWidth:0} {PageHeight:0}] " +
                    $"/Resources {pageResources} " +
                    $"/Contents {contentObjectId} 0 R >>"));

            var streamBytes = Encoding.ASCII.GetBytes(
                pageStreams[index]);

            objects.Add(
                PdfObject.FromStream(
                    streamBytes,
                    string.Empty));
        }

        objects.Add(
            PdfObject.FromText(
                "<< /Type /Font " +
                "/Subtype /Type1 " +
                "/BaseFont /Helvetica " +
                "/Encoding /WinAnsiEncoding >>"));

        objects.Add(
            PdfObject.FromText(
                "<< /Type /Font " +
                "/Subtype /Type1 " +
                "/BaseFont /Helvetica-Bold " +
                "/Encoding /WinAnsiEncoding >>"));

        var alphaReference = alphaObjectId.HasValue
            ? $" /SMask {alphaObjectId.Value} 0 R"
            : string.Empty;

        objects.Add(
            PdfObject.FromStream(
                logo.RgbData,
                $"/Type /XObject " +
                $"/Subtype /Image " +
                $"/Width {logo.Width} " +
                $"/Height {logo.Height} " +
                "/ColorSpace /DeviceRGB " +
                "/BitsPerComponent 8 " +
                "/Filter /FlateDecode" +
                alphaReference));

        if (logo.HasAlpha)
        {
            objects.Add(
                PdfObject.FromStream(
                    logo.AlphaData!,
                    $"/Type /XObject " +
                    $"/Subtype /Image " +
                    $"/Width {logo.Width} " +
                    $"/Height {logo.Height} " +
                    "/ColorSpace /DeviceGray " +
                    "/BitsPerComponent 8 " +
                    "/Filter /FlateDecode"));
        }

        using var output = new MemoryStream();

        WriteAscii(
            output,
            "%PDF-1.4\n");

        output.Write(
            [0x25, 0xE2, 0xE3, 0xCF, 0xD3, 0x0A]);

        var offsets = new List<long>
        {
            0
        };

        for (var index = 0;
             index < objects.Count;
             index++)
        {
            offsets.Add(output.Position);

            WriteAscii(
                output,
                $"{index + 1} 0 obj\n");

            output.Write(objects[index].Bytes);

            WriteAscii(
                output,
                "\nendobj\n");
        }

        var xrefPosition = output.Position;

        WriteAscii(
            output,
            $"xref\n0 {objects.Count + 1}\n");

        WriteAscii(
            output,
            "0000000000 65535 f \n");

        for (var index = 1;
             index < offsets.Count;
             index++)
        {
            WriteAscii(
                output,
                $"{offsets[index]:0000000000} 00000 n \n");
        }

        WriteAscii(
            output,
            $"trailer << /Size {objects.Count + 1} " +
            "/Root 1 0 R >>\n" +
            $"startxref\n{xrefPosition}\n%%EOF");

        return output.ToArray();
    }

    private static void DrawImage(
        StringBuilder content,
        string resourceName,
        double x,
        double y,
        double width,
        double height)
    {
        content
            .Append("q ")
            .Append(Number(width))
            .Append(" 0 0 ")
            .Append(Number(height))
            .Append(' ')
            .Append(Number(x))
            .Append(' ')
            .Append(Number(y))
            .Append(" cm /")
            .Append(resourceName)
            .Append(" Do Q\n");
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
        content
            .Append(color)
            .Append(" rg ");

        content
            .Append("BT /")
            .Append(bold ? "F2" : "F1")
            .Append(' ')
            .Append(Number(fontSize))
            .Append(" Tf ")
            .Append(Number(x))
            .Append(' ')
            .Append(Number(y))
            .Append(" Td (")
            .Append(EscapePdfText(value))
            .Append(") Tj ET\n");
    }

    private static void DrawRectangle(
        StringBuilder content,
        double x,
        double y,
        double width,
        double height,
        string color)
    {
        content
            .Append(color)
            .Append(" rg ")
            .Append(Number(x))
            .Append(' ')
            .Append(Number(y))
            .Append(' ')
            .Append(Number(width))
            .Append(' ')
            .Append(Number(height))
            .Append(" re f\n");
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
        content
            .Append(color)
            .Append(" RG ")
            .Append(Number(lineWidth))
            .Append(" w ")
            .Append(Number(x))
            .Append(' ')
            .Append(Number(y))
            .Append(' ')
            .Append(Number(width))
            .Append(' ')
            .Append(Number(height))
            .Append(" re S\n");
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
        content
            .Append(color)
            .Append(" RG ")
            .Append(Number(lineWidth))
            .Append(" w ")
            .Append(Number(x1))
            .Append(' ')
            .Append(Number(y1))
            .Append(" m ")
            .Append(Number(x2))
            .Append(' ')
            .Append(Number(y2))
            .Append(" l S\n");
    }

    private static string EscapePdfText(
        string value)
    {
        var normalized = value
            .Replace('–', '-')
            .Replace('—', '-')
            .Replace('“', '"')
            .Replace('”', '"')
            .Replace('’', '\'');

        var bytes = Encoding.Latin1.GetBytes(
            normalized);

        var result = new StringBuilder(
            bytes.Length);

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
                        result
                            .Append('\\')
                            .Append(
                                Convert.ToString(
                                    valueByte,
                                    8)!
                                .PadLeft(3, '0'));
                    }
                    else
                    {
                        result.Append(
                            (char)valueByte);
                    }

                    break;
            }
        }

        return result.ToString();
    }

    private static string Truncate(
        string value,
        int maxCharacters)
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

    private static string Number(
        double value)
    {
        return value.ToString(
            "0.##",
            CultureInfo.InvariantCulture);
    }

    private static void WriteAscii(
        Stream stream,
        string value)
    {
        var bytes = Encoding.ASCII.GetBytes(
            value);

        stream.Write(
            bytes,
            0,
            bytes.Length);
    }

    private sealed record ColumnDefinition(
        string Header,
        double Width,
        int MaxCharacters);

    private sealed record PdfObject(
        byte[] Bytes)
    {
        public static PdfObject FromText(
            string value)
        {
            return new PdfObject(
                Encoding.ASCII.GetBytes(value));
        }

        public static PdfObject FromStream(
            byte[] data,
            string dictionary)
        {
            using var output =
                new MemoryStream();

            WriteAscii(
                output,
                $"<< {dictionary} /Length {data.Length} >>\n" +
                "stream\n");

            output.Write(
                data,
                0,
                data.Length);

            WriteAscii(
                output,
                "\nendstream");

            return new PdfObject(
                output.ToArray());
        }
    }

    private sealed record PngImageData(
        int Width,
        int Height,
        byte[] RgbData,
        byte[]? AlphaData)
    {
        public bool HasAlpha =>
            AlphaData is not null;

        public static PngImageData Load(
            string path)
        {
            var png = File.ReadAllBytes(path);

            ReadOnlySpan<byte> signature =
            [
                137, 80, 78, 71,
                13, 10, 26, 10
            ];

            if (png.Length < signature.Length ||
                !png.AsSpan(
                        0,
                        signature.Length)
                    .SequenceEqual(signature))
            {
                throw new InvalidDataException(
                    "El archivo configurado como logo " +
                    "no es un PNG válido.");
            }

            var position = 8;

            var width = 0;
            var height = 0;
            var bitDepth = 0;
            var colorType = 0;
            var interlace = 0;

            using var idatStream =
                new MemoryStream();

            while (position + 12 <= png.Length)
            {
                var length =
                    BinaryPrimitives
                        .ReadInt32BigEndian(
                            png.AsSpan(
                                position,
                                4));

                position += 4;

                var chunkType =
                    Encoding.ASCII.GetString(
                        png,
                        position,
                        4);

                position += 4;

                if (length < 0 ||
                    position + length + 4 >
                    png.Length)
                {
                    throw new InvalidDataException(
                        "El PNG del logo contiene " +
                        "un chunk inválido.");
                }

                var chunkData =
                    png.AsSpan(
                        position,
                        length);

                switch (chunkType)
                {
                    case "IHDR":
                        width =
                            BinaryPrimitives
                                .ReadInt32BigEndian(
                                    chunkData[..4]);

                        height =
                            BinaryPrimitives
                                .ReadInt32BigEndian(
                                    chunkData.Slice(4, 4));

                        bitDepth = chunkData[8];
                        colorType = chunkData[9];
                        interlace = chunkData[12];
                        break;

                    case "IDAT":
                        idatStream.Write(
                            chunkData);
                        break;

                    case "IEND":
                        position =
                            png.Length;
                        break;
                }

                position += length + 4;
            }

            if (width <= 0 ||
                height <= 0)
            {
                throw new InvalidDataException(
                    "No fue posible obtener las dimensiones " +
                    "del logo PNG.");
            }

            if (bitDepth != 8)
            {
                throw new NotSupportedException(
                    "El logo PNG debe utilizar 8 bits " +
                    "por canal.");
            }

            if (colorType is not 2 and not 6)
            {
                throw new NotSupportedException(
                    "El logo PNG debe estar en formato RGB " +
                    "o RGBA.");
            }

            if (interlace != 0)
            {
                throw new NotSupportedException(
                    "El logo PNG no puede estar entrelazado.");
            }

            var bytesPerPixel =
                colorType == 6 ? 4 : 3;

            var stride =
                checked(width * bytesPerPixel);

            var expectedSize =
                checked(
                    height * (stride + 1));

            idatStream.Position = 0;

            byte[] filtered;

            using (var zlib =
                   new ZLibStream(
                       idatStream,
                       CompressionMode.Decompress))
            using (var decoded =
                   new MemoryStream())
            {
                zlib.CopyTo(decoded);
                filtered = decoded.ToArray();
            }

            if (filtered.Length < expectedSize)
            {
                throw new InvalidDataException(
                    "El contenido del PNG está incompleto.");
            }

            var reconstructed =
                new byte[
                    checked(height * stride)];

            var previousRow =
                new byte[stride];

            var currentRow =
                new byte[stride];

            var sourceOffset = 0;

            for (var row = 0;
                 row < height;
                 row++)
            {
                var filterType =
                    filtered[sourceOffset++];

                Array.Copy(
                    filtered,
                    sourceOffset,
                    currentRow,
                    0,
                    stride);

                sourceOffset += stride;

                ApplyPngFilter(
                    currentRow,
                    previousRow,
                    bytesPerPixel,
                    filterType);

                Buffer.BlockCopy(
                    currentRow,
                    0,
                    reconstructed,
                    row * stride,
                    stride);

                (previousRow, currentRow) =
                    (currentRow, previousRow);
            }

            var rgb =
                new byte[
                    checked(width * height * 3)];

            byte[]? alpha =
                colorType == 6
                    ? new byte[
                        checked(width * height)]
                    : null;

            var sourceIndex = 0;
            var rgbIndex = 0;
            var alphaIndex = 0;

            for (var pixel = 0;
                 pixel < width * height;
                 pixel++)
            {
                rgb[rgbIndex++] =
                    reconstructed[sourceIndex++];

                rgb[rgbIndex++] =
                    reconstructed[sourceIndex++];

                rgb[rgbIndex++] =
                    reconstructed[sourceIndex++];

                if (colorType == 6)
                {
                    alpha![alphaIndex++] =
                        reconstructed[sourceIndex++];
                }
            }

            return new PngImageData(
                width,
                height,
                Compress(rgb),
                alpha is null
                    ? null
                    : Compress(alpha));
        }

        private static void ApplyPngFilter(
            byte[] current,
            byte[] previous,
            int bytesPerPixel,
            byte filterType)
        {
            for (var index = 0;
                 index < current.Length;
                 index++)
            {
                var left =
                    index >= bytesPerPixel
                        ? current[
                            index -
                            bytesPerPixel]
                        : 0;

                var above =
                    previous[index];

                var upperLeft =
                    index >= bytesPerPixel
                        ? previous[
                            index -
                            bytesPerPixel]
                        : 0;

                current[index] =
                    filterType switch
                    {
                        0 => current[index],

                        1 => unchecked(
                            (byte)(
                                current[index] +
                                left)),

                        2 => unchecked(
                            (byte)(
                                current[index] +
                                above)),

                        3 => unchecked(
                            (byte)(
                                current[index] +
                                ((left + above) / 2))),

                        4 => unchecked(
                            (byte)(
                                current[index] +
                                PaethPredictor(
                                    left,
                                    above,
                                    upperLeft))),

                        _ => throw new InvalidDataException(
                            $"Filtro PNG no soportado: {filterType}.")
                    };
            }
        }

        private static byte PaethPredictor(
            int left,
            int above,
            int upperLeft)
        {
            var prediction =
                left + above - upperLeft;

            var distanceLeft =
                Math.Abs(prediction - left);

            var distanceAbove =
                Math.Abs(prediction - above);

            var distanceUpperLeft =
                Math.Abs(
                    prediction -
                    upperLeft);

            if (distanceLeft <= distanceAbove &&
                distanceLeft <=
                distanceUpperLeft)
            {
                return (byte)left;
            }

            if (distanceAbove <=
                distanceUpperLeft)
            {
                return (byte)above;
            }

            return (byte)upperLeft;
        }

        private static byte[] Compress(
            byte[] data)
        {
            using var output =
                new MemoryStream();

            using (var zlib =
                   new ZLibStream(
                       output,
                       CompressionLevel.Optimal,
                       leaveOpen: true))
            {
                zlib.Write(
                    data,
                    0,
                    data.Length);
            }

            return output.ToArray();
        }
    }
}