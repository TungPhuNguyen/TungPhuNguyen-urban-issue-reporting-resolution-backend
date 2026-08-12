namespace UrbanIssue.Application.Features.Dashboard.Common;

public interface IDashboardDateRangeQuery
{
    DateOnly? FromDate { get; }

    DateOnly? ToDate { get; }
}
