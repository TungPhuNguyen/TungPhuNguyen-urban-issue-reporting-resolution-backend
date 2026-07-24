using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanIssue.Infrastructure.Sqlserver.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDashboardReportIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Reports_ClosedAt",
                table: "Reports",
                column: "ClosedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_CreatedAt",
                table: "Reports",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ResolvedAt",
                table: "Reports",
                column: "ResolvedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_SLAStartedAt",
                table: "Reports",
                column: "SLAStartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_Status_DueAt",
                table: "Reports",
                columns: new[] { "Status", "DueAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reports_ClosedAt",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_CreatedAt",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_ResolvedAt",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_SLAStartedAt",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_Status_DueAt",
                table: "Reports");
        }
    }
}
