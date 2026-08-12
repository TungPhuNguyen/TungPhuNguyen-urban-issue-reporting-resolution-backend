namespace UrbanIssue.Application.Features.Reports.CheckDuplicateReports;

public interface IReportDuplicateChecker
{
    Task<CheckDuplicateReportsResult> CheckAsync(
        int categoryId,
        decimal latitude,
        decimal longitude,
        CancellationToken cancellationToken,
        Guid? excludedReportId = null);
}
