using AuditCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditCore.Infrastructure.Persistence.Configurations;

public sealed class FindingConfiguration
    : IEntityTypeConfiguration<Finding>
{
    public void Configure(
        EntityTypeBuilder<Finding> builder)
    {
        builder.ToTable("Findings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Condition)
            .HasMaxLength(3000)
            .IsRequired();

        builder.Property(x => x.Criteria)
            .HasMaxLength(3000)
            .IsRequired();

        builder.Property(x => x.Cause)
            .HasMaxLength(3000);

        builder.Property(x => x.Effect)
            .HasMaxLength(3000);

        builder.Property(x => x.Recommendation)
            .HasMaxLength(3000);

        builder.Property(x => x.Severity)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(x => x.RowVersion)
            .IsRowVersion();

        builder.HasIndex(x => new
        {
            x.AuditId,
            x.Code
        })
            .IsUnique();

        builder.HasOne(x => x.Audit)
            .WithMany()
            .HasForeignKey(x => x.AuditId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Risk)
            .WithMany()
            .HasForeignKey(x => x.RiskId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ResponsibleUser)
            .WithMany()
            .HasForeignKey(x => x.ResponsibleUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
