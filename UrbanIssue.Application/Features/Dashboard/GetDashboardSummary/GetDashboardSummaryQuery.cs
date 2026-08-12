using MediatR;
using UrbanIssue.Application.Features.Dashboard.Common;

namespace UrbanIssue.Application.Features.Dashboard.GetDashboardSummary;

public sealed record GetDashboardSummaryQuery(
    DateOnly? FromDate = null,
    DateOnly? ToDate = null)
    : IRequest<DashboardSummaryResult>,
      IDashboardDateRangeQuery;
