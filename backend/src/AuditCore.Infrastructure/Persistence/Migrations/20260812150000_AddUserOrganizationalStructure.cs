using AuditCore.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditCore.Infrastructure.Persistence.Migrations;

[DbContext(typeof(AuditCoreDbContext))]
[Migration("20260812150000_AddUserOrganizationalStructure")]
public partial class AddUserOrganizationalStructure : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<Guid>(
            name: "BranchId",
            table: "Users",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.AddColumn<Guid>(
            name: "DepartmentId",
            table: "Users",
            type: "uniqueidentifier",
            nullable: true);

        migrationBuilder.CreateIndex(
            name: "IX_Users_BranchId",
            table: "Users",
            column: "BranchId");

        migrationBuilder.CreateIndex(
            name: "IX_Users_DepartmentId",
            table: "Users",
            column: "DepartmentId");

        migrationBuilder.AddForeignKey(
            name: "FK_Users_Branches_BranchId",
            table: "Users",
            column: "BranchId",
            principalTable: "Branches",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);

        migrationBuilder.AddForeignKey(
            name: "FK_Users_Departments_DepartmentId",
            table: "Users",
            column: "DepartmentId",
            principalTable: "Departments",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Users_Branches_BranchId",
            table: "Users");

        migrationBuilder.DropForeignKey(
            name: "FK_Users_Departments_DepartmentId",
            table: "Users");

        migrationBuilder.DropIndex(
            name: "IX_Users_BranchId",
            table: "Users");

        migrationBuilder.DropIndex(
            name: "IX_Users_DepartmentId",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "BranchId",
            table: "Users");

        migrationBuilder.DropColumn(
            name: "DepartmentId",
            table: "Users");
    }
}
