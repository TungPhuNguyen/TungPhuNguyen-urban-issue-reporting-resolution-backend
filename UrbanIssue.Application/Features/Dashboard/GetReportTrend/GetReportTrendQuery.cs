using MediatR;
using UrbanIssue.Application.Features.Dashboard.Common;

namespace UrbanIssue.Application.Features.Dashboard.GetReportTrend;

public sealed record GetReportTrendQuery(
    DateOnly? FromDate = null,
    DateOnly? ToDate = null)
    : IRequest<ReportTrendResult>,
      IDashboardDateRangeQuery;
