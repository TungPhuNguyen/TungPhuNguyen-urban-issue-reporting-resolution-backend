using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanIssue.Infrastructure.Sqlserver.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddReportComplaintAndAutoCloseSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Comments_ReportId",
                table: "Comments");

            migrationBuilder.AddColumn<string>(
                name: "ComplaintReason",
                table: "Reports",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ComplaintSubmittedAt",
                table: "Reports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsComplaint",
                table: "Comments",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Comments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_Status_ComplaintSubmittedAt",
                table: "Reports",
                columns: new[] { "Status", "ComplaintSubmittedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reports_Status_ComplaintSubmittedAt",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ComplaintReason",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "ComplaintSubmittedAt",
                table: "Reports");

            migrationBuilder.AlterColumn<bool>(
                name: "IsComplaint",
                table: "Comments",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.CreateIndex(
                name: "IX_Comments_ReportId",
                table: "Comments",
                column: "ReportId",
                unique: true,
                filter: "[IsComplaint] = 1");
        }
    }
}
