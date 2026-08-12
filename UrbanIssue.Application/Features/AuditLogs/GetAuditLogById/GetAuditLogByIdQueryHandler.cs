using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.AuditLogs.Common;

namespace UrbanIssue.Application.Features.AuditLogs.GetAuditLogById;

public sealed class GetAuditLogByIdQueryHandler
    : IRequestHandler<
        GetAuditLogByIdQuery,
        AuditLogDetailResult>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAuditLogByIdQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AuditLogDetailResult> Handle(
        GetAuditLogByIdQuery request,
        CancellationToken cancellationToken)
    {
        var result =
            await _dbContext.AuditLogs
                .AsNoTracking()
                .Where(log =>
                    log.Id == request.Id)
                .Select(log =>
                    new AuditLogDetailResult(
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
                        log.Detail,
                        log.CreatedAt))
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (result is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy Audit Log có ID {request.Id}.");
        }

        return result;
    }
}
