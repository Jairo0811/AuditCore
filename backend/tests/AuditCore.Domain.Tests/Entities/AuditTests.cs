using AuditCore.Domain.Entities;

namespace AuditCore.Domain.Tests.Entities;

public sealed class AuditTests
{
    [Fact]
    public void Constructor_ShouldCreateDraftAudit()
    {
        var audit = new Audit(
            Guid.NewGuid(),
            " aud-001 ",
            " Auditoría Financiera ",
            " Revisar controles ",
            " Área financiera ");

        Assert.Equal("AUD-001", audit.Code);
        Assert.Equal("Auditoría Financiera", audit.Title);
        Assert.Equal("Revisar controles", audit.Objective);
        Assert.Equal("Área financiera", audit.Scope);
        Assert.Equal(AuditStatus.Draft, audit.Status);
        Assert.True(audit.IsActive);
    }

    [Fact]
    public void Plan_ShouldMoveAuditToPlanned()
    {
        var audit = CreateAudit();
        var auditorId = Guid.NewGuid();

        audit.Plan(
            auditorId,
            DateTime.UtcNow.Date,
            DateTime.UtcNow.Date.AddDays(5));

        Assert.Equal(AuditStatus.Planned, audit.Status);
        Assert.Equal(auditorId, audit.LeadAuditorUserId);
    }

    [Fact]
    public void Plan_ShouldThrow_WhenEndDateIsBeforeStartDate()
    {
        var audit = CreateAudit();

        var start = DateTime.UtcNow.Date;

        Assert.Throws<ArgumentException>(() =>
            audit.Plan(
                Guid.NewGuid(),
                start,
                start.AddDays(-1)));
    }

    [Fact]
    public void Audit_ShouldFollowValidLifecycle()
    {
        var audit = CreateAudit();

        audit.Plan(
            Guid.NewGuid(),
            DateTime.UtcNow.Date,
            DateTime.UtcNow.Date.AddDays(5));

        audit.Start();
        Assert.Equal(AuditStatus.InProgress, audit.Status);

        audit.Complete();
        Assert.Equal(AuditStatus.Completed, audit.Status);

        audit.Close();

        Assert.Equal(AuditStatus.Closed, audit.Status);
        Assert.False(audit.IsActive);
    }

    [Fact]
    public void Start_ShouldThrow_WhenAuditIsStillDraft()
    {
        var audit = CreateAudit();

        Assert.Throws<InvalidOperationException>(
            audit.Start);
    }

    [Fact]
    public void Cancel_ShouldDeactivateAudit()
    {
        var audit = CreateAudit();

        audit.Cancel();

        Assert.Equal(AuditStatus.Cancelled, audit.Status);
        Assert.False(audit.IsActive);
    }

    private static Audit CreateAudit()
    {
        return new Audit(
            Guid.NewGuid(),
            "AUD-001",
            "Auditoría de prueba");
    }
}
