using MediatR;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Reports.Admin.Common;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Admin.GetAdminReports;

public sealed record GetAdminReportsQuery(
    string? Search = null,
    ReportStatus? Status = null,
    ReportPriority? Priority = null,
    int? CategoryId = null,
    int? AreaId = null,
    int? DepartmentId = null,
    bool? RequiresManualAssignment = null,
    bool? HasComplaint = null,
    int PageNumber = 1,
    int PageSize = 20)
    : IRequest<PagedResult<AdminReportSummaryResult>>;
