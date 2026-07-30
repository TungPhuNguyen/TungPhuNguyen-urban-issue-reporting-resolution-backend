using MediatR;
using UrbanIssue.Application.Features.Reports.Staff.Common;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Staff.AcceptReport;

public sealed record AcceptReportCommand(
    Guid ReportId,
    ReportPriority Priority,
    string? Note)
    : IRequest<StaffReportActionResult>;
