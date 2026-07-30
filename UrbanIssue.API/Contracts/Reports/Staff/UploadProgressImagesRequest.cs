namespace UrbanIssue.API.Contracts.Reports.Staff;

public sealed class UploadProgressImagesRequest
{
    public string? Note { get; init; }

    public List<IFormFile> Images { get; init; } = [];
}
