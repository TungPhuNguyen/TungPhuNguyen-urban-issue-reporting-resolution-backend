using Microsoft.AspNetCore.Http;

namespace UrbanIssue.API.Contracts.Reports;

public sealed class CreateReportRequest
{
    public int CategoryId { get; init; }

    public int AreaId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string? OtherCategoryText { get; init; }

    public string? AddressText { get; init; }

    public decimal Latitude { get; init; }

    public decimal Longitude { get; init; }

    public bool ConfirmPossibleDuplicate { get; init; }

    public List<IFormFile> Images { get; init; } = [];
}
