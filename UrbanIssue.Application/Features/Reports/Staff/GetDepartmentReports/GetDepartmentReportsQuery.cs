using MediatR;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Reports.Staff.Common;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Staff.GetDepartmentReports;

public sealed record GetDepartmentReportsQuery(
    string? Search = null,
    ReportStatus? Status = null,
    ReportPriority? Priority = null,
    bool? IsOverdue = null,
    bool? IsEscalated = null,
    int PageNumber = 1,
    int PageSize = 20)
    : IRequest<PagedResult<StaffReportSummaryResult>>;
