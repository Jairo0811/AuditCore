using AuditCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditCore.Infrastructure.Persistence.Configurations;

public sealed class EvidenceConfiguration : IEntityTypeConfiguration<Evidence>
{
    public void Configure(EntityTypeBuilder<Evidence> builder)
    {
        builder.ToTable("Evidences");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FileName).HasMaxLength(260).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(150).IsRequired();
        builder.Property(x => x.StorageKey).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Sha256).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => x.StorageKey).IsUnique();
        builder.HasIndex(x => new { x.AuditId, x.Sha256 });
        builder.HasOne(x => x.Audit).WithMany().HasForeignKey(x => x.AuditId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Finding).WithMany().HasForeignKey(x => x.FindingId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.UploadedByUser).WithMany().HasForeignKey(x => x.UploadedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ActionPlanConfiguration : IEntityTypeConfiguration<ActionPlan>
{
    public void Configure(EntityTypeBuilder<ActionPlan> builder)
    {
        builder.ToTable("ActionPlans");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Title).HasMaxLength(250).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(3000);
        builder.Property(x => x.CompletionNotes).HasMaxLength(3000);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => x.FindingId);
        builder.HasIndex(x => new { x.ResponsibleUserId, x.DueDateUtc });
        builder.HasOne(x => x.Finding).WithMany().HasForeignKey(x => x.FindingId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.ResponsibleUser).WithMany().HasForeignKey(x => x.ResponsibleUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ControlFrameworkConfiguration : IEntityTypeConfiguration<ControlFramework>
{
    public void Configure(EntityTypeBuilder<ControlFramework> builder)
    {
        builder.ToTable("ControlFrameworks");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Version).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.Code, x.Version }).IsUnique();
    }
}

public sealed class ControlDefinitionConfiguration : IEntityTypeConfiguration<ControlDefinition>
{
    public void Configure(EntityTypeBuilder<ControlDefinition> builder)
    {
        builder.ToTable("ControlDefinitions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Domain).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(3000);
        builder.Property(x => x.Weight).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.FrameworkId, x.Code }).IsUnique();
        builder.HasOne(x => x.Framework).WithMany(x => x.Controls).HasForeignKey(x => x.FrameworkId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ControlEvaluationConfiguration : IEntityTypeConfiguration<ControlEvaluation>
{
    public void Configure(EntityTypeBuilder<ControlEvaluation> builder)
    {
        builder.ToTable("ControlEvaluations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Notes).HasMaxLength(3000);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.AuditId, x.ControlId }).IsUnique();
        builder.HasOne(x => x.Audit).WithMany().HasForeignKey(x => x.AuditId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Control).WithMany().HasForeignKey(x => x.ControlId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.EvaluatedByUser).WithMany().HasForeignKey(x => x.EvaluatedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ControlQuestionConfiguration : IEntityTypeConfiguration<ControlQuestion>
{
    public void Configure(EntityTypeBuilder<ControlQuestion> builder)
    {
        builder.ToTable("ControlQuestions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Text).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Weight).HasPrecision(5, 2).IsRequired();
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.ControlId, x.Order }).IsUnique();
        builder.HasOne(x => x.Control).WithMany().HasForeignKey(x => x.ControlId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ControlAnswerConfiguration : IEntityTypeConfiguration<ControlAnswer>
{
    public void Configure(EntityTypeBuilder<ControlAnswer> builder)
    {
        builder.ToTable("ControlAnswers");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Notes).HasMaxLength(2000);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.EvaluationId, x.QuestionId }).IsUnique();
        builder.HasOne(x => x.Evaluation).WithMany().HasForeignKey(x => x.EvaluationId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Question).WithMany().HasForeignKey(x => x.QuestionId).OnDelete(DeleteBehavior.Restrict);
    }
}
