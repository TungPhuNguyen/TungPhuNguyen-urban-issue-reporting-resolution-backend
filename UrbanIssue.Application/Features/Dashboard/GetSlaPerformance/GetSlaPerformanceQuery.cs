using MediatR;
using UrbanIssue.Application.Features.Dashboard.Common;

namespace UrbanIssue.Application.Features.Dashboard.GetSlaPerformance;

public sealed record GetSlaPerformanceQuery(
    DateOnly? FromDate = null,
    DateOnly? ToDate = null)
    : IRequest<SlaPerformanceResult>,
      IDashboardDateRangeQuery;
