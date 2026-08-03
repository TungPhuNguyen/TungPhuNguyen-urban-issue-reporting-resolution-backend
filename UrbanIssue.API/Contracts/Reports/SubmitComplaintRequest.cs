namespace UrbanIssue.API.Contracts.Reports;

public sealed class SubmitComplaintRequest
{
    public string Reason { get; init; } = string.Empty;
    public List<IFormFile> Images { get; init; } = [];
}
