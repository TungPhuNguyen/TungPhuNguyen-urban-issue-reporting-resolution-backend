using Microsoft.AspNetCore.Mvc;
using UrbanIssue.API.Common.ModelBinding;

namespace UrbanIssue.API.Contracts.Reports;

public sealed class CreateReportRequest
{
    public int CategoryId { get; init; }

    public int AreaId { get; init; }

    public string Description { get; init; } =
        string.Empty;

    public string? AddressText { get; init; }

    [ModelBinder(BinderType = typeof(InvariantDecimalModelBinder))]
    public decimal Latitude { get; init; }

    [ModelBinder(BinderType = typeof(InvariantDecimalModelBinder))]
    public decimal Longitude { get; init; }

    public List<IFormFile> Images { get; init; } =
        [];
}
