using System;
using AuditCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditCore.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AuditCoreDbContext))]
[Migration("20260807180000_CompleteAuditExecution")]
public partial class CompleteAuditExecution : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ControlFrameworks",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Version = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            }, constraints: table => table.PrimaryKey("PK_ControlFrameworks", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Findings",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AuditId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RiskId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                Condition = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                Criteria = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: false),
                Cause = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                Effect = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                Recommendation = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                ResponsibleUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                DueDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Findings", x => x.Id);
                table.ForeignKey("FK_Findings_Audits_AuditId", x => x.AuditId, "Audits", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Findings_Risks_RiskId", x => x.RiskId, "Risks", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Findings_Users_ResponsibleUserId", x => x.ResponsibleUserId, "Users", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ControlDefinitions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FrameworkId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                Domain = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                Weight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_ControlDefinitions", x => x.Id);
                table.ForeignKey("FK_ControlDefinitions_ControlFrameworks_FrameworkId", x => x.FrameworkId, "ControlFrameworks", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ActionPlans",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FindingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Title = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                Description = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                ResponsibleUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                DueDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                ProgressPercent = table.Column<int>(type: "int", nullable: false),
                Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                CompletionNotes = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                CompletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_ActionPlans", x => x.Id);
                table.ForeignKey("FK_ActionPlans_Findings_FindingId", x => x.FindingId, "Findings", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_ActionPlans_Users_ResponsibleUserId", x => x.ResponsibleUserId, "Users", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Evidences",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AuditId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FindingId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                FileName = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: false),
                ContentType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                StorageKey = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                Sha256 = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Evidences", x => x.Id);
                table.ForeignKey("FK_Evidences_Audits_AuditId", x => x.AuditId, "Audits", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Evidences_Findings_FindingId", x => x.FindingId, "Findings", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Evidences_Users_UploadedByUserId", x => x.UploadedByUserId, "Users", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ControlEvaluations",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AuditId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ControlId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Score = table.Column<int>(type: "int", nullable: true),
                Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                Notes = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                EvaluatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                EvaluatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_ControlEvaluations", x => x.Id);
                table.ForeignKey("FK_ControlEvaluations_Audits_AuditId", x => x.AuditId, "Audits", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_ControlEvaluations_ControlDefinitions_ControlId", x => x.ControlId, "ControlDefinitions", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_ControlEvaluations_Users_EvaluatedByUserId", x => x.EvaluatedByUserId, "Users", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_ControlFrameworks_Code_Version", "ControlFrameworks", new[] { "Code", "Version" }, unique: true);
        migrationBuilder.CreateIndex("IX_Findings_AuditId_Code", "Findings", new[] { "AuditId", "Code" }, unique: true);
        migrationBuilder.CreateIndex("IX_Findings_RiskId", "Findings", "RiskId");
        migrationBuilder.CreateIndex("IX_Findings_ResponsibleUserId", "Findings", "ResponsibleUserId");
        migrationBuilder.CreateIndex("IX_ControlDefinitions_FrameworkId_Code", "ControlDefinitions", new[] { "FrameworkId", "Code" }, unique: true);
        migrationBuilder.CreateIndex("IX_ActionPlans_FindingId", "ActionPlans", "FindingId");
        migrationBuilder.CreateIndex("IX_ActionPlans_ResponsibleUserId_DueDateUtc", "ActionPlans", new[] { "ResponsibleUserId", "DueDateUtc" });
        migrationBuilder.CreateIndex("IX_Evidences_StorageKey", "Evidences", "StorageKey", unique: true);
        migrationBuilder.CreateIndex("IX_Evidences_AuditId_Sha256", "Evidences", new[] { "AuditId", "Sha256" });
        migrationBuilder.CreateIndex("IX_Evidences_FindingId", "Evidences", "FindingId");
        migrationBuilder.CreateIndex("IX_Evidences_UploadedByUserId", "Evidences", "UploadedByUserId");
        migrationBuilder.CreateIndex("IX_ControlEvaluations_AuditId_ControlId", "ControlEvaluations", new[] { "AuditId", "ControlId" }, unique: true);
        migrationBuilder.CreateIndex("IX_ControlEvaluations_ControlId", "ControlEvaluations", "ControlId");
        migrationBuilder.CreateIndex("IX_ControlEvaluations_EvaluatedByUserId", "ControlEvaluations", "EvaluatedByUserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("ActionPlans");
        migrationBuilder.DropTable("ControlEvaluations");
        migrationBuilder.DropTable("Evidences");
        migrationBuilder.DropTable("ControlDefinitions");
        migrationBuilder.DropTable("Findings");
        migrationBuilder.DropTable("ControlFrameworks");
    }
}
