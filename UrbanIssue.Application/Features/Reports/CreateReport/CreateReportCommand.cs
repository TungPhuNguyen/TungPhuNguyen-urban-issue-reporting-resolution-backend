using MediatR;
using UrbanIssue.Application.Common.Models;

namespace UrbanIssue.Application.Features.Reports.CreateReport;

public sealed record CreateReportCommand(
    int CategoryId,
    int AreaId,
    string Title,
    string Description,
    string? OtherCategoryText,
    string? AddressText,
    decimal Latitude,
    decimal Longitude,
    bool ConfirmPossibleDuplicate,
    IReadOnlyList<UploadFile> Images)
    : IRequest<CreateReportResult>;
