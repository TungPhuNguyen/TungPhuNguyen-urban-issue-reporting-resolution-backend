using MediatR;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.AuditLogs.Common;

namespace UrbanIssue.Application.Features.AuditLogs.GetAuditLogs;

public sealed record GetAuditLogsQuery(
    Guid? UserId = null,
    string? Action = null,
    string? EntityType = null,
    string? EntityId = null,
    DateTime? CreatedFrom = null,
    DateTime? CreatedTo = null,
    int PageNumber = 1,
    int PageSize = 20)
    : IRequest<PagedResult<AuditLogSummaryResult>>;
