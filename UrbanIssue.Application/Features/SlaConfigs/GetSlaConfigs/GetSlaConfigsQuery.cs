using MediatR;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.SlaConfigs.Common;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.SlaConfigs.GetSlaConfigs;

public sealed record GetSlaConfigsQuery(
    string? Search = null,
    int? CategoryId = null,
    ReportPriority? Priority = null,
    int PageNumber = 1,
    int PageSize = 20)
    : IRequest<PagedResult<SlaConfigResult>>;
