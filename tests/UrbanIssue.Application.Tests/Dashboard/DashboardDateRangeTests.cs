using UrbanIssue.Application.Features.Dashboard.Common;
using UrbanIssue.Application.Features.Dashboard.GetReportsByArea;

namespace UrbanIssue.Application.Tests.Dashboard;

public sealed class DashboardDateRangeTests
{
    [Fact]
    public void Resolve_CreatesInclusiveUtcDateRange()
    {
        var result = DashboardDateRangeResolver.Resolve(
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 31));

        Assert.Equal(
            new DateTime(
                2026,
                7,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc),
            result.FromUtc);

        Assert.Equal(
            new DateTime(
                2026,
                8,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc),
            result.ToExclusiveUtc);
    }

    [Fact]
    public async Task Validator_RejectsFromDateAfterToDate()
    {
        var validator =
            new GetReportsByAreaQueryValidator();

        var result = await validator.ValidateAsync(
            new GetReportsByAreaQuery(
                FromDate: new DateOnly(2026, 7, 31),
                ToDate: new DateOnly(2026, 7, 1)));

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage.Contains(
                "FromDate phải nhỏ hơn hoặc bằng ToDate",
                StringComparison.Ordinal));
    }

    [Fact]
    public async Task Validator_RejectsPeriodLongerThan366Days()
    {
        var validator =
            new GetReportsByAreaQueryValidator();

        var result = await validator.ValidateAsync(
            new GetReportsByAreaQuery(
                FromDate: new DateOnly(2025, 1, 1),
                ToDate: new DateOnly(2026, 1, 2)));

        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors,
            error => error.ErrorMessage.Contains(
                "không được vượt quá 366 ngày",
                StringComparison.Ordinal));
    }
}
