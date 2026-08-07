using System;
using AuditCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditCore.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AuditCoreDbContext))]
[Migration("20260807180500_AddControlQuestions")]
public partial class AddControlQuestions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "ControlQuestions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ControlId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                Weight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                Order = table.Column<int>(type: "int", nullable: false),
                IsRequired = table.Column<bool>(type: "bit", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ControlQuestions", x => x.Id);
                table.ForeignKey(
                    name: "FK_ControlQuestions_ControlDefinitions_ControlId",
                    column: x => x.ControlId,
                    principalTable: "ControlDefinitions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ControlAnswers",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                EvaluationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                QuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Score = table.Column<int>(type: "int", nullable: true),
                Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                UpdatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                DeletedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ControlAnswers", x => x.Id);
                table.ForeignKey(
                    name: "FK_ControlAnswers_ControlEvaluations_EvaluationId",
                    column: x => x.EvaluationId,
                    principalTable: "ControlEvaluations",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_ControlAnswers_ControlQuestions_QuestionId",
                    column: x => x.QuestionId,
                    principalTable: "ControlQuestions",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_ControlQuestions_ControlId_Order",
            table: "ControlQuestions",
            columns: new[] { "ControlId", "Order" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ControlAnswers_EvaluationId_QuestionId",
            table: "ControlAnswers",
            columns: new[] { "EvaluationId", "QuestionId" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_ControlAnswers_QuestionId",
            table: "ControlAnswers",
            column: "QuestionId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ControlAnswers");
        migrationBuilder.DropTable(name: "ControlQuestions");
    }
}
