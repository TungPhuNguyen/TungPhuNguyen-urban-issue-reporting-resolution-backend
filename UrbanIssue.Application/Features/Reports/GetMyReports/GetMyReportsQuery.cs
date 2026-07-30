using MediatR;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Reports.Common;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.GetMyReports;

public sealed record GetMyReportsQuery(
    string? Search = null,
    ReportStatus? Status = null,
    int PageNumber = 1,
    int PageSize = 20)
    : IRequest<PagedResult<CitizenReportSummaryResult>>;
