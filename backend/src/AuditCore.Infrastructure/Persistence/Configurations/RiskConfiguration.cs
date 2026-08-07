using AuditCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditCore.Infrastructure.Persistence.Configurations;

public sealed class RiskConfiguration
    : IEntityTypeConfiguration<Risk>
{
    public void Configure(
        EntityTypeBuilder<Risk> builder)
    {
        builder.ToTable("Risks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.Treatment)
            .HasMaxLength(2000);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Ignore(x => x.Score);
        builder.Ignore(x => x.Level);

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

        builder.HasOne(x => x.OwnerUser)
            .WithMany()
            .HasForeignKey(x => x.OwnerUserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
