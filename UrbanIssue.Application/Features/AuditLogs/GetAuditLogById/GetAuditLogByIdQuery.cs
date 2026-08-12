using MediatR;
using UrbanIssue.Application.Features.AuditLogs.Common;

namespace UrbanIssue.Application.Features.AuditLogs.GetAuditLogById;

public sealed record GetAuditLogByIdQuery(
    int Id)
    : IRequest<AuditLogDetailResult>;
