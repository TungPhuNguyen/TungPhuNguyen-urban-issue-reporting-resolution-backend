using MediatR;
using UrbanIssue.Application.Features.Reports.Common;

namespace UrbanIssue.Application.Features.Reports.UpdateReport;

public sealed record UpdateReportCommand(
    Guid ReportId,
    int CategoryId,
    int AreaId,
    string Title,
    string Description,
    string? OtherCategoryText,
    string? AddressText,
    decimal Latitude,
    decimal Longitude,
    bool ConfirmPossibleDuplicate,
    byte[] RowVersion)
    : IRequest<CitizenReportDetailResult>;
