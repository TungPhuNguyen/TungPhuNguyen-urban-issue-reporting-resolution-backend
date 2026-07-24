using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanIssue.Infrastructure.Sqlserver.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPublicReportMapIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Reports_CategoryId_AreaId_Status_CreatedAt",
                table: "Reports",
                columns: new[] { "CategoryId", "AreaId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Reports_Status_Latitude_Longitude",
                table: "Reports",
                columns: new[] { "Status", "Latitude", "Longitude" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reports_CategoryId_AreaId_Status_CreatedAt",
                table: "Reports");

            migrationBuilder.DropIndex(
                name: "IX_Reports_Status_Latitude_Longitude",
                table: "Reports");
        }
    }
}
