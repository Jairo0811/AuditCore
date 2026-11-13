namespace AuditCore.Infrastructure.Services;

/// <summary>
/// Reglas de validación de archivos de evidencia: extensión permitida, coherencia con el MIME declarado,
/// firma del contenido y resolución segura de rutas dentro del almacenamiento.
/// </summary>
public static class EvidenceFileRules
{
    private sealed record FileKind(string CanonicalContentType, string[] AcceptedContentTypes, Func<byte[], bool> ContentMatches);

    private static readonly byte[] PdfSignature = "%PDF-"u8.ToArray();
    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] ZipSignature = [0x50, 0x4B, 0x03, 0x04];

    private static readonly Dictionary<string, FileKind> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        [".pdf"] = new("application/pdf", ["application/pdf"], c => StartsWith(c, PdfSignature)),
        [".png"] = new("image/png", ["image/png"], c => StartsWith(c, PngSignature)),
        [".jpg"] = new("image/jpeg", ["image/jpeg", "image/pjpeg"], c => StartsWith(c, JpegSignature)),
        [".jpeg"] = new("image/jpeg", ["image/jpeg", "image/pjpeg"], c => StartsWith(c, JpegSignature)),
        [".txt"] = new("text/plain", ["text/plain"], IsText),
        [".csv"] = new("text/csv", ["text/csv", "text/plain", "application/vnd.ms-excel"], IsText),
        [".xlsx"] = new(
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"],
            c => StartsWith(c, ZipSignature)),
        [".docx"] = new(
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"],
            c => StartsWith(c, ZipSignature))
    };

    public static IReadOnlyCollection<string> AllowedFileExtensions => AllowedExtensions.Keys;

    /// <summary>
    /// Devuelve el tipo de contenido canónico que se almacenará para el archivo o lanza
    /// <see cref="InvalidOperationException"/> si la extensión, el MIME declarado o el contenido no son coherentes.
    /// </summary>
    public static string ResolveContentType(string fileName, string? declaredContentType, byte[] content)
    {
        var extension = Path.GetExtension(fileName);
        if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.TryGetValue(extension, out var kind))
            throw new InvalidOperationException("El tipo de archivo no está permitido. Formatos admitidos: PDF, PNG, JPG, TXT, CSV, XLSX y DOCX.");

        var normalizedDeclared = (declaredContentType ?? string.Empty).Split(';')[0].Trim();
        if (normalizedDeclared.Length > 0
            && !string.Equals(normalizedDeclared, "application/octet-stream", StringComparison.OrdinalIgnoreCase)
            && !kind.AcceptedContentTypes.Contains(normalizedDeclared, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException("El tipo MIME declarado no coincide con la extensión del archivo.");

        if (!kind.ContentMatches(content))
            throw new InvalidOperationException("El contenido del archivo no coincide con su extensión.");

        return kind.CanonicalContentType;
    }

    /// <summary>
    /// Resuelve una clave de almacenamiento a una ruta física, garantizando que permanezca dentro de la raíz.
    /// Usa una ruta relativa normalizada en lugar de un prefijo de texto para evitar falsos positivos
    /// como <c>&lt;raíz&gt;-otra-cosa</c>.
    /// </summary>
    public static string ResolveSafePath(string storageRoot, string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey) || Path.IsPathRooted(storageKey))
            throw new InvalidOperationException("Ruta de almacenamiento inválida.");

        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(storageRoot));
        var fullPath = Path.GetFullPath(Path.Combine(root, storageKey.Replace('/', Path.DirectorySeparatorChar)));
        var relative = Path.GetRelativePath(root, fullPath);

        if (relative == "."
            || Path.IsPathRooted(relative)
            || relative == ".."
            || relative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            || relative.StartsWith("../", StringComparison.Ordinal))
            throw new InvalidOperationException("Ruta de almacenamiento inválida.");

        return fullPath;
    }

    private static bool StartsWith(byte[] content, byte[] signature) =>
        content.Length >= signature.Length && content.AsSpan(0, signature.Length).SequenceEqual(signature);

    /// <summary>Archivos de texto: se rechazan si contienen bytes nulos (indicador típico de contenido binario).</summary>
    private static bool IsText(byte[] content) => Array.IndexOf(content, (byte)0) < 0;
}
