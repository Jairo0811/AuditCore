using AuditCore.Domain.Entities;

namespace AuditCore.Domain.Tests.Entities;

public sealed class RiskTests
{
    [Fact]
    public void Constructor_ShouldCalculateLowRisk()
    {
        var risk = CreateRisk(
            probability: 2,
            impact: 2);

        Assert.Equal(4, risk.Score);
        Assert.Equal(RiskLevel.Low, risk.Level);
        Assert.Equal(RiskStatus.Identified, risk.Status);
        Assert.True(risk.IsActive);
    }

    [Fact]
    public void Constructor_ShouldCalculateCriticalRisk()
    {
        var risk = CreateRisk(
            probability: 5,
            impact: 5);

        Assert.Equal(25, risk.Score);
        Assert.Equal(RiskLevel.Critical, risk.Level);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenProbabilityIsInvalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateRisk(
                probability: 0,
                impact: 3));
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenImpactIsInvalid()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateRisk(
                probability: 3,
                impact: 6));
    }

    [Fact]
    public void Risk_ShouldFollowTreatmentLifecycle()
    {
        var risk = CreateRisk(
            probability: 4,
            impact: 4);

        risk.StartTreatment();

        Assert.Equal(
            RiskStatus.UnderTreatment,
            risk.Status);

        risk.Mitigate();

        Assert.Equal(
            RiskStatus.Mitigated,
            risk.Status);

        risk.Close();

        Assert.Equal(
            RiskStatus.Closed,
            risk.Status);

        Assert.False(risk.IsActive);
    }

    [Fact]
    public void AcceptedRisk_ShouldBeClosable()
    {
        var risk = CreateRisk(
            probability: 3,
            impact: 3);

        risk.Accept();
        risk.Close();

        Assert.Equal(
            RiskStatus.Closed,
            risk.Status);

        Assert.False(risk.IsActive);
    }

    private static Risk CreateRisk(
        int probability,
        int impact)
    {
        return new Risk(
            Guid.NewGuid(),
            "RSK-001",
            "Riesgo de prueba",
            "Descripción",
            probability,
            impact,
            "Mitigar el riesgo");
    }
}
