using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using UrbanIssue.Infrastructure.Sqlserver.Persistence;

#nullable disable

namespace UrbanIssue.Infrastructure.Sqlserver.Persistence.Migrations;

[DbContext(typeof(ApplicationDbContext))]
[Migration("20260803090000_P0P1Improvements")]
public partial class P0P1Improvements : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateSequence<long>(
            name: "ReportNumbers",
            startValue: 100001L);

        migrationBuilder.AddColumn<bool>(
            name: "IsOther",
            table: "Categories",
            type: "bit",
            nullable: false,
            defaultValue: false);

        migrationBuilder.AddColumn<string>(
            name: "EventType",
            table: "StatusUpdates",
            type: "nvarchar(50)",
            maxLength: 50,
            nullable: false,
            defaultValue: "StatusChanged");

        migrationBuilder.AddColumn<long>(
            name: "ReportNumber",
            table: "Reports",
            type: "bigint",
            nullable: false,
            defaultValueSql: "NEXT VALUE FOR [ReportNumbers]");

        migrationBuilder.AddColumn<string>(
            name: "ReportCode",
            table: "Reports",
            type: "nvarchar(30)",
            maxLength: 30,
            nullable: true,
            computedColumnSql: "('UI-' + CONVERT([varchar](20),[ReportNumber]))",
            stored: true);

        migrationBuilder.AddColumn<string>(
            name: "Title",
            table: "Reports",
            type: "nvarchar(150)",
            maxLength: 150,
            nullable: true);

        migrationBuilder.AddColumn<string>(
            name: "OtherCategoryText",
            table: "Reports",
            type: "nvarchar(250)",
            maxLength: 250,
            nullable: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "CancelledAt",
            table: "Reports",
            type: "datetime2",
            nullable: true);

        migrationBuilder.AddColumn<byte[]>(
            name: "RowVersion",
            table: "Reports",
            type: "rowversion",
            rowVersion: true,
            nullable: false);

        migrationBuilder.Sql(
            """
            UPDATE [Reports]
            SET [Title] = LEFT(
                CASE
                    WHEN LEN(LTRIM(RTRIM([Description]))) >= 10
                        THEN LTRIM(RTRIM([Description]))
                    ELSE N'Phản ánh đô thị'
                END,
                150)
            WHERE [Title] IS NULL;
            """);

        migrationBuilder.AlterColumn<string>(
            name: "Title",
            table: "Reports",
            type: "nvarchar(150)",
            maxLength: 150,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "nvarchar(150)",
            oldMaxLength: 150,
            oldNullable: true);

        migrationBuilder.CreateTable(
            name: "Complaints",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ReportId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CitizenId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                AdminDecisionReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                ResolvedByAdminId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Complaints", x => x.Id);
                table.ForeignKey(
                    name: "FK_Complaints_Reports_ReportId",
                    column: x => x.ReportId,
                    principalTable: "Reports",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_Complaints_Users_CitizenId",
                    column: x => x.CitizenId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_Complaints_Users_ResolvedByAdminId",
                    column: x => x.ResolvedByAdminId,
                    principalTable: "Users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "ComplaintImages",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                ComplaintId = table.Column<int>(type: "int", nullable: false),
                ImageUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ComplaintImages", x => x.Id);
                table.ForeignKey(
                    name: "FK_ComplaintImages_Complaints_ComplaintId",
                    column: x => x.ComplaintId,
                    principalTable: "Complaints",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.Sql(
            """
            INSERT INTO [Complaints]
                ([ReportId], [CitizenId], [Reason], [Status], [CreatedAt], [ResolvedAt])
            SELECT
                [Id],
                [CitizenId],
                COALESCE(NULLIF([ComplaintReason], N''), N'Dữ liệu khiếu nại được chuyển từ phiên bản cũ.'),
                CASE
                    WHEN [ComplaintSubmittedAt] IS NOT NULL THEN N'Pending'
                    WHEN [Status] = N'InProgress' THEN N'Accepted'
                    WHEN [Status] = N'Closed' THEN N'Rejected'
                    ELSE N'Rejected'
                END,
                COALESCE([ComplaintSubmittedAt], [UpdatedAt], [CreatedAt]),
                CASE WHEN [ComplaintSubmittedAt] IS NULL THEN COALESCE([UpdatedAt], [CreatedAt]) ELSE NULL END
            FROM [Reports]
            WHERE [HasSubmittedComplaint] = 1;
            """);

        migrationBuilder.Sql(
            """
            IF NOT EXISTS (SELECT 1 FROM [Categories] WHERE [IsOther] = 1)
            BEGIN
                INSERT INTO [Categories]
                    ([Name], [Description], [IsActive], [IsOther], [CreatedAt], [UpdatedAt])
                VALUES
                    (N'Khác / Tôi không chắc',
                     N'Admin sẽ phân loại trước khi giao đơn vị xử lý.',
                     1, 1, SYSUTCDATETIME(), NULL);
            END
            """);

        migrationBuilder.CreateIndex(
            name: "IX_Categories_IsOther",
            table: "Categories",
            column: "IsOther",
            unique: true,
            filter: "[IsOther] = 1");

        migrationBuilder.CreateIndex(
            name: "IX_Reports_ReportNumber",
            table: "Reports",
            column: "ReportNumber",
            unique: true);

        migrationBuilder.Sql(
    """
    CREATE UNIQUE INDEX [IX_Reports_ReportCode]
        ON [Reports] ([ReportCode]);
    """);

        migrationBuilder.CreateIndex(
            name: "IX_ComplaintImages_ComplaintId",
            table: "ComplaintImages",
            column: "ComplaintId");

        migrationBuilder.CreateIndex(
            name: "IX_Complaints_CitizenId",
            table: "Complaints",
            column: "CitizenId");

        migrationBuilder.CreateIndex(
            name: "IX_Complaints_ReportId",
            table: "Complaints",
            column: "ReportId",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Complaints_ResolvedByAdminId",
            table: "Complaints",
            column: "ResolvedByAdminId");

        migrationBuilder.CreateIndex(
            name: "IX_Complaints_Status_CreatedAt",
            table: "Complaints",
            columns: new[] { "Status", "CreatedAt" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "ComplaintImages");
        migrationBuilder.DropTable(name: "Complaints");
        migrationBuilder.DropIndex(name: "IX_Categories_IsOther", table: "Categories");
        migrationBuilder.DropIndex(name: "IX_Reports_ReportCode", table: "Reports");
        migrationBuilder.DropIndex(name: "IX_Reports_ReportNumber", table: "Reports");
        migrationBuilder.DropColumn(name: "IsOther", table: "Categories");
        migrationBuilder.DropColumn(name: "EventType", table: "StatusUpdates");
        migrationBuilder.DropColumn(name: "ReportCode", table: "Reports");
        migrationBuilder.DropColumn(name: "ReportNumber", table: "Reports");
        migrationBuilder.DropColumn(name: "Title", table: "Reports");
        migrationBuilder.DropColumn(name: "OtherCategoryText", table: "Reports");
        migrationBuilder.DropColumn(name: "CancelledAt", table: "Reports");
        migrationBuilder.DropColumn(name: "RowVersion", table: "Reports");
        migrationBuilder.DropSequence(name: "ReportNumbers");
    }
}
