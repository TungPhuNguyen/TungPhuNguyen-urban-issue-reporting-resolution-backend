using Microsoft.EntityFrameworkCore;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;
using UrbanIssue.Infrastructure.Sqlserver.Persistence;

namespace UrbanIssue.Application.Tests.Dashboard;

internal static class DashboardTestDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(
                    $"dashboard-tests-{Guid.NewGuid():N}")
                .Options;

        return new ApplicationDbContext(options);
    }

    public static async Task SeedAsync(
        ApplicationDbContext dbContext)
    {
        var role = new Role
        {
            Id = 1,
            Name = "Citizen",
            CreatedAt = DateTime.UtcNow
        };

        var citizen = new User
        {
            Id = Guid.Parse(
                "11111111-1111-1111-1111-111111111111"),
            FullName = "Citizen Test",
            Email = "citizen.dashboard@test.local",
            PasswordHash = "not-used",
            RoleId = role.Id,
            Role = role,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var roadCategory = new Category
        {
            Id = 1,
            Name = "Đường giao thông",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var lightingCategory = new Category
        {
            Id = 2,
            Name = "Chiếu sáng",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var wardA = new Area
        {
            Id = 1,
            Name = "Phường A",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var wardB = new Area
        {
            Id = 2,
            Name = "Phường B",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var wardC = new Area
        {
            Id = 3,
            Name = "Phường C",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Roles.Add(role);
        dbContext.Users.Add(citizen);
        dbContext.Categories.AddRange(
            roadCategory,
            lightingCategory);
        dbContext.Areas.AddRange(
            wardA,
            wardB,
            wardC);

        dbContext.Reports.AddRange(
            CreateReport(
                citizen,
                roadCategory,
                wardA,
                createdAt: Utc(2026, 7, 2, 7),
                slaStartedAt: Utc(2026, 7, 2, 8),
                resolvedAt: Utc(2026, 7, 2, 12)),

            CreateReport(
                citizen,
                roadCategory,
                wardA,
                createdAt: Utc(2026, 7, 3, 9),
                slaStartedAt: Utc(2026, 7, 3, 10),
                resolvedAt: Utc(2026, 7, 3, 16)),

            CreateReport(
                citizen,
                lightingCategory,
                wardB,
                createdAt: Utc(2026, 7, 4, 8),
                slaStartedAt: Utc(2026, 7, 4, 9),
                resolvedAt: null,
                status: ReportStatus.InProgress),

            CreateReport(
                citizen,
                roadCategory,
                wardB,
                createdAt: Utc(2026, 7, 4, 10),
                slaStartedAt: Utc(2026, 7, 4, 11),
                resolvedAt: Utc(2026, 7, 4, 21)),

            CreateReport(
                citizen,
                lightingCategory,
                wardA,
                createdAt: Utc(2026, 8, 1, 8),
                slaStartedAt: Utc(2026, 8, 1, 9),
                resolvedAt: Utc(2026, 8, 1, 11)),

            CreateReport(
                citizen,
                lightingCategory,
                wardC,
                createdAt: Utc(2026, 9, 1, 8),
                slaStartedAt: Utc(2026, 9, 1, 9),
                resolvedAt: null,
                status: ReportStatus.InProgress));

        await dbContext.SaveChangesAsync();
    }

    private static Report CreateReport(
        User citizen,
        Category category,
        Area area,
        DateTime createdAt,
        DateTime? slaStartedAt,
        DateTime? resolvedAt,
        ReportStatus status = ReportStatus.Resolved)
    {
        return new Report
        {
            Id = Guid.NewGuid(),
            CitizenId = citizen.Id,
            Citizen = citizen,
            CategoryId = category.Id,
            Category = category,
            AreaId = area.Id,
            Area = area,
            Description = "Dữ liệu kiểm thử dashboard",
            Latitude = 21.028511m,
            Longitude = 105.804817m,
            Status = status,
            CreatedAt = createdAt,
            SLAStartedAt = slaStartedAt,
            ResolvedAt = resolvedAt
        };
    }

    private static DateTime Utc(
        int year,
        int month,
        int day,
        int hour)
    {
        return new DateTime(
            year,
            month,
            day,
            hour,
            0,
            0,
            DateTimeKind.Utc);
    }
}
