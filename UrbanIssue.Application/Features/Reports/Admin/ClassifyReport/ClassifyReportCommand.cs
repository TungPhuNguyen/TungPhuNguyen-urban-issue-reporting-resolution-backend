using MediatR;
using UrbanIssue.Application.Features.Reports.Admin.Common;

namespace UrbanIssue.Application.Features.Reports.Admin.ClassifyReport;

public sealed record ClassifyReportCommand(
    Guid ReportId,
    int CategoryId,
    string? Note)
    : IRequest<AdminReportActionResult>;
