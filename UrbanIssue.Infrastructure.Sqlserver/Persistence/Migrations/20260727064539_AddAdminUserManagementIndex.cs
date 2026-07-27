using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UrbanIssue.Infrastructure.Sqlserver.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminUserManagementIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_RoleId",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId_DepartmentId_IsActive_CreatedAt",
                table: "Users",
                columns: new[] { "RoleId", "DepartmentId", "IsActive", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_RoleId_DepartmentId_IsActive_CreatedAt",
                table: "Users");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
        }
    }
}
