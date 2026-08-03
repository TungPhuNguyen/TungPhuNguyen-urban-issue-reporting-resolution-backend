using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Auditing;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Notifications;
using UrbanIssue.Application.Common.Rules;
using UrbanIssue.Application.Features.Reports.CheckDuplicateReports;
using UrbanIssue.Application.Features.Reports.PostResolution.CloseReport;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;
using UrbanIssue.Infrastructure.Sqlserver.Persistence;

namespace UrbanIssue.Application.Tests.Reports;

public sealed class CriticalBusinessRulesTests
{
    [Theory]
    [InlineData(21.028511, 105.804817, true)]
    [InlineData(0, 0, false)]
    [InlineData(10.762622, 106.660172, false)]
    public void Hanoi_location_rule_rejects_invalid_coordinates(
        double latitude,
        double longitude,
        bool expected)
    {
        Assert.Equal(expected, HanoiLocationRules.IsInsideHanoi(
            (decimal)latitude,
            (decimal)longitude));
    }

    [Fact]
    public async Task Close_report_rejects_pending_complaint_for_admin_and_citizen()
    {
        await using var dbContext = CreateDbContext();
        var citizenId = Guid.NewGuid();
        var report = CreateResolvedReport(citizenId);
        dbContext.Reports.Add(report);
        dbContext.Complaints.Add(new Complaint
        {
            ReportId = report.Id,
            CitizenId = citizenId,
            Reason = "Kết quả xử lý chưa khắc phục sự cố.",
            Status = ComplaintStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var handler = new CloseReportCommandHandler(
            dbContext,
            new TestCurrentUser(citizenId, "Citizen"),
            new NullAuditLogService(),
            new NullNotificationService());

        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(
            new CloseReportCommand(report.Id, null),
            CancellationToken.None));
    }

    [Fact]
    public async Task Duplicate_checker_excludes_current_report_when_editing()
    {
        await using var dbContext = CreateDbContext();
        var citizenId = Guid.NewGuid();
        var report = CreateResolvedReport(citizenId);
        report.Status = ReportStatus.New;
        dbContext.Reports.Add(report);
        await dbContext.SaveChangesAsync();

        var checker = new ReportDuplicateChecker(dbContext);
        var result = await checker.CheckAsync(
            report.CategoryId,
            report.Latitude,
            report.Longitude,
            CancellationToken.None,
            report.Id);

        Assert.False(result.HasPossibleDuplicates);
        Assert.Empty(result.Reports);
    }

    private static ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"critical-rules-{Guid.NewGuid():N}")
            .Options;
        return new ApplicationDbContext(options);
    }

    private static Report CreateResolvedReport(Guid citizenId)
    {
        return new Report
        {
            Id = Guid.NewGuid(),
            ReportNumber = Random.Shared.NextInt64(100001, 999999),
            ReportCode = $"UI-{Random.Shared.Next(100001, 999999)}",
            CitizenId = citizenId,
            CategoryId = 1,
            AreaId = 1,
            Title = "Ổ gà nguy hiểm trước cổng trường",
            Description = "Mặt đường hư hỏng gây nguy hiểm cho người đi đường.",
            Latitude = 21.028511m,
            Longitude = 105.804817m,
            Status = ReportStatus.Resolved,
            ResolvedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
    }

    private sealed class TestCurrentUser : ICurrentUserService
    {
        public TestCurrentUser(Guid userId, string role)
        {
            UserId = userId;
            Role = role;
        }

        public Guid UserId { get; }
        public bool IsAuthenticated => true;
        public string? Role { get; }
    }

    private sealed class NullAuditLogService : IAuditLogService
    {
        public void Add(
            Guid? userId,
            string action,
            string entityType,
            string entityId,
            object? detail = null)
        {
        }
    }

    private sealed class NullNotificationService : INotificationService
    {
        public void Add(
            Guid userId,
            Guid? reportId,
            NotificationType type,
            string title,
            string message,
            DateTime createdAt)
        {
        }

        public void AddMany(
            IEnumerable<Guid> userIds,
            Guid? reportId,
            NotificationType type,
            string title,
            string message,
            DateTime createdAt)
        {
        }
    }
}
