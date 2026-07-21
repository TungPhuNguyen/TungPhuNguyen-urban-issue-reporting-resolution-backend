using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanIssue.Infrastructure.Sqlserver.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRoutingRuleUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RoutingRules_CategoryId_AreaId_IsActive",
                table: "RoutingRules");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_RoutingRules_CategoryId_AreaId_IsActive",
                table: "RoutingRules",
                columns: new[] { "CategoryId", "AreaId", "IsActive" });
        }
    }
}
