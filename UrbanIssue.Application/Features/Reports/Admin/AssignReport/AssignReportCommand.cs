using MediatR;
using UrbanIssue.Application.Features.Reports.Admin.Common;

namespace UrbanIssue.Application.Features.Reports.Admin.AssignReport;

public sealed record AssignReportCommand(
    Guid ReportId,
    int DepartmentId,
    Guid? StaffId,
    string? Note)
    : IRequest<AdminReportActionResult>;
