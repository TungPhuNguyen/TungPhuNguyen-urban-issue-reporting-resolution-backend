using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Reports.Staff.Dashboard.Common;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Staff.Dashboard.GetStaffDashboard;

public sealed class GetStaffDashboardQueryHandler
    : IRequestHandler<
        GetStaffDashboardQuery,
        StaffDashboardResult>
{
    private const int DefaultRangeInDays = 30;

    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetStaffDashboardQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<StaffDashboardResult> Handle(
        GetStaffDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var currentTime = DateTime.UtcNow;

        var to = request.To ?? currentTime;
        var from =
            request.From
            ?? to.AddDays(-DefaultRangeInDays);

        var staffId = _currentUserService.UserId;

        var staff = await _dbContext.Users
            .AsNoTracking()
            .Where(user =>
                user.Id == staffId
                && user.IsActive
                && user.Role.Name == "Staff")
            .Select(user => new
            {
                user.DepartmentId
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (staff is null)
        {
            throw new UnauthorizedAccessException(
                "Tài khoản Staff không hợp lệ "
                + "hoặc đã bị khóa.");
        }

        if (!staff.DepartmentId.HasValue)
        {
            throw new ConflictException(
                "Tài khoản Staff chưa được gán Department.");
        }

        var departmentId =
            staff.DepartmentId.Value;

        var departmentName =
            await _dbContext.Departments
                .AsNoTracking()
                .Where(department =>
                    department.Id == departmentId)
                .Select(department =>
                    department.Name)
                .SingleOrDefaultAsync(cancellationToken);

        if (departmentName is null)
        {
            throw new ConflictException(
                "Department của Staff không tồn tại.");
        }

        var rows = await _dbContext.Reports
            .AsNoTracking()
            .Where(report =>
                report.DepartmentId == departmentId
                && report.CreatedAt >= from
                && report.CreatedAt <= to)
            .Select(report =>
                new DashboardReportRow(
                    report.Status,
                    report.CreatedAt,
                    report.AcceptedAt,
                    report.ResolvedAt,
                    report.DueAt,
                    report.SLAWarningSentAt,
                    report.SLABreachedNotifiedAt,
                    report.IsEscalated))
            .ToListAsync(cancellationToken);

        var statusCounts = rows
            .GroupBy(row => row.Status)
            .ToDictionary(
                group => group.Key,
                group => group.Count());

        int CountStatus(ReportStatus status)
        {
            return statusCounts.TryGetValue(
                status,
                out var count)
                ? count
                : 0;
        }

        var activeRows = rows.Where(row =>
            row.Status == ReportStatus.Assigned
            || row.Status == ReportStatus.Accepted
            || row.Status == ReportStatus.InProgress)
            .ToList();

        var resolutionHours = rows
            .Where(row =>
                row.AcceptedAt.HasValue
                && row.ResolvedAt.HasValue
                && row.ResolvedAt.Value
                    >= row.AcceptedAt.Value)
            .Select(row =>
                (
                    row.ResolvedAt!.Value
                    - row.AcceptedAt!.Value
                ).TotalHours)
            .ToList();

        double? averageResolutionHours =
            resolutionHours.Count == 0
                ? null
                : Math.Round(
                    resolutionHours.Average(),
                    2);

        var reportsByStatus =
            Enum.GetValues<ReportStatus>()
                .Select(status =>
                    new StaffDashboardStatusItem(
                        Status: status,
                        Count: CountStatus(status)))
                .ToList();

        var trend = rows
            .GroupBy(row => row.CreatedAt.Date)
            .OrderBy(group => group.Key)
            .Select(group =>
                new StaffDashboardTrendItem(
                    Date: group.Key,
                    Count: group.Count()))
            .ToList();

        return new StaffDashboardResult(
            DepartmentId:
                departmentId,
            DepartmentName:
                departmentName,
            From:
                from,
            To:
                to,
            TotalReports:
                rows.Count,
            NewReports:
                CountStatus(ReportStatus.New),
            AssignedReports:
                CountStatus(ReportStatus.Assigned),
            AcceptedReports:
                CountStatus(ReportStatus.Accepted),
            InProgressReports:
                CountStatus(ReportStatus.InProgress),
            ResolvedReports:
                CountStatus(ReportStatus.Resolved),
            ClosedReports:
                CountStatus(ReportStatus.Closed),
            RejectedReports:
                CountStatus(ReportStatus.Rejected),
            SlaWarningReports:
                activeRows.Count(row =>
                    row.SLAWarningSentAt.HasValue
                    && !row.SLABreachedNotifiedAt.HasValue),
            SlaBreachedReports:
                activeRows.Count(row =>
                    row.SLABreachedNotifiedAt.HasValue),
            EscalatedReports:
                activeRows.Count(row =>
                    row.IsEscalated),
            OverdueReports:
                activeRows.Count(row =>
                    row.DueAt.HasValue
                    && row.DueAt.Value < currentTime),
            AverageResolutionHours:
                averageResolutionHours,
            ReportsByStatus:
                reportsByStatus,
            Trend:
                trend);
    }

    private sealed record DashboardReportRow(
        ReportStatus Status,
        DateTime CreatedAt,
        DateTime? AcceptedAt,
        DateTime? ResolvedAt,
        DateTime? DueAt,
        DateTime? SLAWarningSentAt,
        DateTime? SLABreachedNotifiedAt,
        bool IsEscalated);
}
