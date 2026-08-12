using Microsoft.AspNetCore.Mvc;
using UrbanIssue.API.Common.ModelBinding;

namespace UrbanIssue.API.Contracts.Reports;

public sealed class UpdateReportRequest
{
    public int CategoryId { get; init; }
    public int AreaId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string? OtherCategoryText { get; init; }
    public string? AddressText { get; init; }

    [ModelBinder(BinderType = typeof(InvariantDecimalModelBinder))]
    public decimal Latitude { get; init; }

    [ModelBinder(BinderType = typeof(InvariantDecimalModelBinder))]
    public decimal Longitude { get; init; }

    public bool ConfirmPossibleDuplicate { get; init; }
    public byte[] RowVersion { get; init; } = [];
}
