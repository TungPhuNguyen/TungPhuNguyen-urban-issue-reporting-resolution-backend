using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanIssue.Infrastructure.Sqlserver.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddComplaintSubmissionHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasSubmittedComplaint",
                table: "Reports",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
                         """
                         UPDATE Reports
                         SET HasSubmittedComplaint = 1
                         WHERE ComplaintSubmittedAt IS NOT NULL;
                         """);

            migrationBuilder.CreateIndex(
                name: "IX_Reports_HasSubmittedComplaint_Status",
                table: "Reports",
                columns: new[] { "HasSubmittedComplaint", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reports_HasSubmittedComplaint_Status",
                table: "Reports");

            migrationBuilder.DropColumn(
                name: "HasSubmittedComplaint",
                table: "Reports");
        }
    }
}
