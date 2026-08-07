using AuditCore.Domain.Entities;

namespace AuditCore.Domain.Tests.Entities;

public sealed class AuditExecutionModuleTests
{
    [Fact]
    public void Evidence_ShouldNormalizeHashAndActivate()
    {
        var evidence = new Evidence(Guid.NewGuid(), "report.pdf", "application/pdf", 10, "audit/file.pdf", new string('a', 64));
        Assert.True(evidence.IsActive);
        Assert.Equal(new string('A', 64), evidence.Sha256);
    }

    [Fact]
    public void ActionPlan_ShouldFollowProgressLifecycle()
    {
        var plan = new ActionPlan(Guid.NewGuid(), "Remediar acceso", null, Guid.NewGuid(), DateTime.UtcNow.AddDays(10));
        plan.SetProgress(40);
        Assert.Equal(ActionPlanStatus.InProgress, plan.Status);
        Assert.Equal(40, plan.ProgressPercent);
        plan.Complete("Validado");
        Assert.Equal(ActionPlanStatus.Completed, plan.Status);
        Assert.Equal(100, plan.ProgressPercent);
        Assert.False(plan.IsActive);
    }

    [Fact]
    public void ActionPlan_ShouldRejectInvalidProgress()
    {
        var plan = new ActionPlan(Guid.NewGuid(), "Plan", null, Guid.NewGuid(), DateTime.UtcNow.AddDays(1));
        Assert.Throws<ArgumentOutOfRangeException>(() => plan.SetProgress(101));
    }

    [Fact]
    public void ControlFramework_ShouldNormalizeCode()
    {
        var framework = new ControlFramework("COBIT", " cobit ", "2019");
        Assert.Equal("COBIT", framework.Code);
    }

    [Fact]
    public void ControlDefinition_ShouldValidateWeight()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ControlDefinition(Guid.NewGuid(), "APO01", "Control", "APO", 0));
    }

    [Fact]
    public void ControlQuestion_ShouldValidateOrder()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ControlQuestion(Guid.NewGuid(), "¿Existe el control?", 25, 0));
    }

    [Fact]
    public void ControlAnswer_ShouldValidateScore()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ControlAnswer(Guid.NewGuid(), Guid.NewGuid(), 101));
    }

    [Fact]
    public void Evaluation_ShouldAcceptNotApplicableWithoutScore()
    {
        var evaluation = new ControlEvaluation(Guid.NewGuid(), Guid.NewGuid());
        evaluation.Evaluate(null, ComplianceStatus.NotApplicable, "No aplica", Guid.NewGuid());
        Assert.Equal(ComplianceStatus.NotApplicable, evaluation.Status);
        Assert.Null(evaluation.Score);
        Assert.NotNull(evaluation.EvaluatedAtUtc);
    }

    [Fact]
    public void Evaluation_ShouldRejectMissingScoreForCompliance()
    {
        var evaluation = new ControlEvaluation(Guid.NewGuid(), Guid.NewGuid());
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            evaluation.Evaluate(null, ComplianceStatus.Compliant, null, Guid.NewGuid()));
    }
}
