using AuditCore.Domain.Entities;

namespace AuditCore.Domain.Tests.Entities;

public sealed class FindingTests
{
    [Fact]
    public void Constructor_ShouldCreateOpenFinding()
    {
        var finding = CreateFinding();

        Assert.Equal("FND-001", finding.Code);
        Assert.Equal(FindingStatus.Open, finding.Status);
        Assert.True(finding.IsActive);
    }

    [Fact]
    public void Finding_ShouldFollowLifecycle()
    {
        var finding = CreateFinding();

        finding.SendToReview();
        finding.Accept();
        finding.Resolve();
        finding.Close();

        Assert.Equal(FindingStatus.Closed, finding.Status);
        Assert.False(finding.IsActive);
    }

    [Fact]
    public void Accept_ShouldThrow_WhenFindingIsOpen()
    {
        var finding = CreateFinding();

        Assert.Throws<InvalidOperationException>(
            finding.Accept);
    }

    [Fact]
    public void Close_ShouldThrow_WhenFindingIsNotResolved()
    {
        var finding = CreateFinding();

        Assert.Throws<InvalidOperationException>(
            finding.Close);
    }

    [Fact]
    public void Update_ShouldNormalizeCode()
    {
        var finding = CreateFinding();

        finding.Update(
            " fnd-002 ",
            "Hallazgo actualizado",
            "Condición actualizada",
            "Criterio actualizado",
            null,
            null,
            null,
            FindingSeverity.High,
            null,
            null,
            null);

        Assert.Equal("FND-002", finding.Code);
        Assert.Equal(
            FindingSeverity.High,
            finding.Severity);
    }

    private static Finding CreateFinding()
    {
        return new Finding(
            Guid.NewGuid(),
            "FND-001",
            "Hallazgo de prueba",
            "Condición encontrada",
            "Criterio esperado",
            "Causa",
            "Efecto",
            "Recomendación",
            FindingSeverity.Medium);
    }
}
