using UrbanIssue.Application.Features.Dashboard.GetReportsByArea;

namespace UrbanIssue.Application.Tests.Dashboard;

public sealed class GetReportsByAreaQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsCountPercentageAndAverageHandlingHours()
    {
        await using var dbContext =
            DashboardTestDbContextFactory.Create();

        await DashboardTestDbContextFactory.SeedAsync(
            dbContext);

        var handler =
            new GetReportsByAreaQueryHandler(dbContext);

        var result = await handler.Handle(
            new GetReportsByAreaQuery(
                FromDate: new DateOnly(2026, 7, 1),
                ToDate: new DateOnly(2026, 7, 31)),
            CancellationToken.None);

        Assert.Equal(2, result.Count);

        var wardA = Assert.Single(
            result,
            item => item.AreaName == "Phường A");

        Assert.Equal(2, wardA.ReportCount);
        Assert.Equal(50m, wardA.Percentage);
        Assert.Equal(5d, wardA.AverageHandlingHours);

        var wardB = Assert.Single(
            result,
            item => item.AreaName == "Phường B");

        Assert.Equal(2, wardB.ReportCount);
        Assert.Equal(50m, wardB.Percentage);
        Assert.Equal(10d, wardB.AverageHandlingHours);
    }

    [Fact]
    public async Task Handle_ExcludesReportsOutsideRequestedDateRange()
    {
        await using var dbContext =
            DashboardTestDbContextFactory.Create();

        await DashboardTestDbContextFactory.SeedAsync(
            dbContext);

        var handler =
            new GetReportsByAreaQueryHandler(dbContext);

        var result = await handler.Handle(
            new GetReportsByAreaQuery(
                FromDate: new DateOnly(2026, 8, 1),
                ToDate: new DateOnly(2026, 8, 1)),
            CancellationToken.None);

        var item = Assert.Single(result);

        Assert.Equal("Phường A", item.AreaName);
        Assert.Equal(1, item.ReportCount);
        Assert.Equal(100m, item.Percentage);
        Assert.Equal(2d, item.AverageHandlingHours);
    }

    [Fact]
    public async Task Handle_ReturnsNullAverageWhenAreaHasNoCompletedReport()
    {
        await using var dbContext =
            DashboardTestDbContextFactory.Create();

        await DashboardTestDbContextFactory.SeedAsync(
            dbContext);

        var handler =
            new GetReportsByAreaQueryHandler(dbContext);

        var result = await handler.Handle(
            new GetReportsByAreaQuery(
                FromDate: new DateOnly(2026, 9, 1),
                ToDate: new DateOnly(2026, 9, 1)),
            CancellationToken.None);

        var wardC = Assert.Single(result);

        Assert.Equal("Phường C", wardC.AreaName);
        Assert.Equal(1, wardC.ReportCount);
        Assert.Equal(100m, wardC.Percentage);
        Assert.Null(wardC.AverageHandlingHours);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyListWhenRangeHasNoReports()
    {
        await using var dbContext =
            DashboardTestDbContextFactory.Create();

        await DashboardTestDbContextFactory.SeedAsync(
            dbContext);

        var handler =
            new GetReportsByAreaQueryHandler(dbContext);

        var result = await handler.Handle(
            new GetReportsByAreaQuery(
                FromDate: new DateOnly(2025, 1, 1),
                ToDate: new DateOnly(2025, 1, 31)),
            CancellationToken.None);

        Assert.Empty(result);
    }
}
