using MediatR;
using UrbanIssue.Application.Features.Reports.Staff.Common;

namespace UrbanIssue.Application.Features.Reports.Staff.StartProcessingReport;

public sealed record StartProcessingReportCommand(
    Guid ReportId,
    string? Note)
    : IRequest<StaffReportActionResult>;
