using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.AuditLogs.Common;

namespace UrbanIssue.Application.Features.AuditLogs.GetAuditLogs;

public sealed class GetAuditLogsQueryHandler
    : IRequestHandler<
        GetAuditLogsQuery,
        PagedResult<AuditLogSummaryResult>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAuditLogsQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<AuditLogSummaryResult>>
        Handle(
            GetAuditLogsQuery request,
            CancellationToken cancellationToken)
    {
        var query =
            _dbContext.AuditLogs
                .AsNoTracking()
                .AsQueryable();

        if (request.UserId.HasValue)
        {
            query = query.Where(log =>
                log.UserId == request.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Action))
        {
            var action =
                request.Action.Trim();

            query = query.Where(log =>
                log.Action.Contains(action));
        }

        if (!string.IsNullOrWhiteSpace(
            request.EntityType))
        {
            var entityType =
                request.EntityType.Trim();

            query = query.Where(log =>
                log.EntityType == entityType);
        }

        if (!string.IsNullOrWhiteSpace(
            request.EntityId))
        {
            var entityId =
                request.EntityId.Trim();

            query = query.Where(log =>
                log.EntityId == entityId);
        }

        if (request.CreatedFrom.HasValue)
        {
            query = query.Where(log =>
                log.CreatedAt >= request.CreatedFrom.Value);
        }

        if (request.CreatedTo.HasValue)
        {
            query = query.Where(log =>
                log.CreatedAt <= request.CreatedTo.Value);
        }

        var totalItems =
            await query.CountAsync(
                cancellationToken);

        var totalPages =
            totalItems == 0
                ? 0
                : (int)Math.Ceiling(
                    totalItems
                    / (double)request.PageSize);

        var items =
            await query
                .OrderByDescending(log =>
                    log.CreatedAt)
                .ThenByDescending(log =>
                    log.Id)
                .Skip(
                    (request.PageNumber - 1)
                    * request.PageSize)
                .Take(request.PageSize)
                .Select(log =>
                    new AuditLogSummaryResult(
                        log.Id,
                        log.UserId,

                        log.User == null
                            ? null
                            : log.User.FullName,

                        log.User == null
                            ? null
                            : log.User.Email,

                        log.Action,
                        log.EntityType,
                        log.EntityId,
                        log.CreatedAt))
                .ToListAsync(
                    cancellationToken);

        return new PagedResult<AuditLogSummaryResult>(
            Items: items,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize,
            TotalItems: totalItems,
            TotalPages: totalPages);
    }
}
