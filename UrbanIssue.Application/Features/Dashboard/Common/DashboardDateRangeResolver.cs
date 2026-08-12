namespace UrbanIssue.Application.Features.Dashboard.Common;

public readonly record struct ResolvedDashboardDateRange(
    DateOnly FromDate,
    DateOnly ToDate,
    DateTime FromUtc,
    DateTime ToExclusiveUtc);

public static class DashboardDateRangeResolver
{
    private const int DefaultPeriodInDays = 30;

    public static ResolvedDashboardDateRange Resolve(
        DateOnly? fromDate,
        DateOnly? toDate)
    {
        var currentDate = DateOnly.FromDateTime(
            DateTime.UtcNow);

        var resolvedToDate =
            toDate ?? currentDate;

        var resolvedFromDate =
            fromDate
            ?? resolvedToDate.AddDays(
                -(DefaultPeriodInDays - 1));

        var fromUtc =
            resolvedFromDate.ToDateTime(
                TimeOnly.MinValue,
                DateTimeKind.Utc);

        var toExclusiveUtc =
            resolvedToDate
                .AddDays(1)
                .ToDateTime(
                    TimeOnly.MinValue,
                    DateTimeKind.Utc);

        return new ResolvedDashboardDateRange(
            FromDate: resolvedFromDate,
            ToDate: resolvedToDate,
            FromUtc: fromUtc,
            ToExclusiveUtc: toExclusiveUtc);
    }
}
