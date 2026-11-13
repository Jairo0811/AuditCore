using System.Text;
using AuditCore.Infrastructure.Services;

namespace AuditCore.Api.IntegrationTests.Evidence;

public sealed class EvidenceFileRulesTests
{
    private static readonly string Root = Path.Combine(Path.GetTempPath(), "auditcore-rules", "storage");

    [Fact]
    public void ResolveSafePath_ShouldAllowDescendantPaths()
    {
        var path = EvidenceFileRules.ResolveSafePath(Root, "audit/file.txt");

        Assert.Equal(Path.Combine(Root, "audit", "file.txt"), path);
    }

    [Theory]
    [InlineData("../storage-otra-cosa/file.txt")]
    [InlineData("../file.txt")]
    [InlineData("audit/../../file.txt")]
    [InlineData("..")]
    [InlineData(".")]
    [InlineData("")]
    public void ResolveSafePath_ShouldRejectPathsOutsideRoot(string storageKey)
    {
        Assert.Throws<InvalidOperationException>(() => EvidenceFileRules.ResolveSafePath(Root, storageKey));
    }

    [Fact]
    public void ResolveSafePath_ShouldRejectSiblingDirectoryWithSamePrefix()
    {
        var sibling = Root + "-otra-cosa";
        var key = Path.GetRelativePath(Root, Path.Combine(sibling, "file.txt"));

        Assert.Throws<InvalidOperationException>(() => EvidenceFileRules.ResolveSafePath(Root, key));
    }

    [Fact]
    public void ResolveSafePath_ShouldRejectAbsolutePaths()
    {
        var absolute = Path.Combine(Path.GetTempPath(), "other", "file.txt");

        Assert.Throws<InvalidOperationException>(() => EvidenceFileRules.ResolveSafePath(Root, absolute));
    }

    [Theory]
    [InlineData("informe.pdf", "application/pdf", "%PDF-1.7 contenido", "application/pdf")]
    [InlineData("notas.txt", "text/plain", "hola", "text/plain")]
    [InlineData("datos.csv", "application/vnd.ms-excel", "a,b\n1,2", "text/csv")]
    [InlineData("datos.CSV", "", "a,b", "text/csv")]
    [InlineData("libro.xlsx", "application/octet-stream", "PK\u0003\u0004resto", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    public void ResolveContentType_ShouldReturnCanonicalType(string fileName, string declared, string content, string expected)
    {
        var result = EvidenceFileRules.ResolveContentType(fileName, declared, Encoding.Latin1.GetBytes(content));

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("pagina.html", "text/html", "<script></script>")]
    [InlineData("sin-extension", "text/plain", "hola")]
    [InlineData("imagen.png", "image/png", "no es png")]
    [InlineData("informe.pdf", "text/plain", "%PDF-1.7")]
    [InlineData("binario.txt", "text/plain", "a\0b")]
    public void ResolveContentType_ShouldRejectInvalidFiles(string fileName, string declared, string content)
    {
        Assert.Throws<InvalidOperationException>(() =>
            EvidenceFileRules.ResolveContentType(fileName, declared, Encoding.Latin1.GetBytes(content)));
    }
}
