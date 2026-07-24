namespace UrbanIssue.API.Contracts.Reports.Staff;

public sealed class ResolveReportRequest
{
    public string Note { get; init; } = string.Empty;

    public List<IFormFile> Images { get; init; } = [];
}
