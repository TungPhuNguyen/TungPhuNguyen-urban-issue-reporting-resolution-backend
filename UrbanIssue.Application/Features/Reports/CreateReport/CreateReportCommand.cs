using MediatR;
using UrbanIssue.Application.Common.Models;

namespace UrbanIssue.Application.Features.Reports.CreateReport;

public sealed record CreateReportCommand(
    int CategoryId,
    int AreaId,
    string Description,
    string? AddressText,
    decimal Latitude,
    decimal Longitude,
    IReadOnlyList<UploadFile> Images)
    : IRequest<CreateReportResult>;
