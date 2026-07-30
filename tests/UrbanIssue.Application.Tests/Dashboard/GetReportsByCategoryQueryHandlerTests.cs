using UrbanIssue.Application.Features.Dashboard.GetReportsByCategory;

namespace UrbanIssue.Application.Tests.Dashboard;

public sealed class GetReportsByCategoryQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsCountPercentageAndAverageHandlingHours()
    {
        await using var dbContext =
            DashboardTestDbContextFactory.Create();

        await DashboardTestDbContextFactory.SeedAsync(
            dbContext);

        var handler =
            new GetReportsByCategoryQueryHandler(dbContext);

        var result = await handler.Handle(
            new GetReportsByCategoryQuery(
                FromDate: new DateOnly(2026, 7, 1),
                ToDate: new DateOnly(2026, 7, 31)),
            CancellationToken.None);

        Assert.Equal(2, result.Count);

        var road = Assert.Single(
            result,
            item => item.CategoryName
                == "Đường giao thông");

        Assert.Equal(3, road.ReportCount);
        Assert.Equal(75m, road.Percentage);
        Assert.Equal(6.67d, road.AverageHandlingHours);

        var lighting = Assert.Single(
            result,
            item => item.CategoryName == "Chiếu sáng");

        Assert.Equal(1, lighting.ReportCount);
        Assert.Equal(25m, lighting.Percentage);
        Assert.Null(lighting.AverageHandlingHours);
    }

    [Fact]
    public async Task Handle_ExcludesReportsOutsideRequestedDateRange()
    {
        await using var dbContext =
            DashboardTestDbContextFactory.Create();

        await DashboardTestDbContextFactory.SeedAsync(
            dbContext);

        var handler =
            new GetReportsByCategoryQueryHandler(dbContext);

        var result = await handler.Handle(
            new GetReportsByCategoryQuery(
                FromDate: new DateOnly(2026, 8, 1),
                ToDate: new DateOnly(2026, 8, 1)),
            CancellationToken.None);

        var item = Assert.Single(result);

        Assert.Equal("Chiếu sáng", item.CategoryName);
        Assert.Equal(1, item.ReportCount);
        Assert.Equal(100m, item.Percentage);
        Assert.Equal(2d, item.AverageHandlingHours);
    }

    [Fact]
    public async Task Handle_ReturnsNullAverageWhenCategoryHasNoCompletedReport()
    {
        await using var dbContext =
            DashboardTestDbContextFactory.Create();

        await DashboardTestDbContextFactory.SeedAsync(
            dbContext);

        var handler =
            new GetReportsByCategoryQueryHandler(dbContext);

        var result = await handler.Handle(
            new GetReportsByCategoryQuery(
                FromDate: new DateOnly(2026, 7, 4),
                ToDate: new DateOnly(2026, 7, 4)),
            CancellationToken.None);

        var lighting = Assert.Single(
            result,
            item => item.CategoryName == "Chiếu sáng");

        Assert.Null(lighting.AverageHandlingHours);
    }

    [Fact]
    public async Task Handle_ReturnsEmptyListWhenRangeHasNoReports()
    {
        await using var dbContext =
            DashboardTestDbContextFactory.Create();

        await DashboardTestDbContextFactory.SeedAsync(
            dbContext);

        var handler =
            new GetReportsByCategoryQueryHandler(dbContext);

        var result = await handler.Handle(
            new GetReportsByCategoryQuery(
                FromDate: new DateOnly(2025, 1, 1),
                ToDate: new DateOnly(2025, 1, 31)),
            CancellationToken.None);

        Assert.Empty(result);
    }
}
