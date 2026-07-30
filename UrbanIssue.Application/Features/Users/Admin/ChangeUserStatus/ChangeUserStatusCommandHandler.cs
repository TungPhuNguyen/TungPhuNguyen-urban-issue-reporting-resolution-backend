using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Constants;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Auditing;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Users.Admin.Common;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Users.Admin.ChangeUserStatus;

public sealed class ChangeUserStatusCommandHandler
    : IRequestHandler<
        ChangeUserStatusCommand,
        UserStatusActionResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLogService;

    public ChangeUserStatusCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IAuditLogService auditLogService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _auditLogService = auditLogService;
    }

    public async Task<UserStatusActionResult> Handle(
        ChangeUserStatusCommand request,
        CancellationToken cancellationToken)
    {
        var adminId = _currentUserService.UserId;

        var user = await _dbContext.Users
            .Include(item => item.Role)
            .SingleOrDefaultAsync(
                item => item.Id == request.UserId,
                cancellationToken);

        if (user is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy người dùng có ID {request.UserId}.");
        }

        if (user.IsActive == request.IsActive)
        {
            throw new ConflictException(
                request.IsActive
                    ? "Tài khoản đã ở trạng thái hoạt động."
                    : "Tài khoản đã bị khóa.");
        }

        if (!request.IsActive && user.Id == adminId)
        {
            throw new ConflictException(
                "Admin không thể khóa chính tài khoản đang đăng nhập.");
        }

        if (!request.IsActive
            && user.Role.Name == "Staff")
        {
            var hasActiveReports = await _dbContext.Reports
                .AsNoTracking()
                .AnyAsync(
                    report =>
                        report.AssignedStaffId == user.Id
                        && (
                            report.Status == ReportStatus.Assigned
                            || report.Status == ReportStatus.Accepted
                            || report.Status == ReportStatus.InProgress
                        ),
                    cancellationToken);

            if (hasActiveReports)
            {
                throw new ConflictException(
                    "Không thể khóa Staff đang giữ báo cáo chưa hoàn tất. "
                    + "Hãy reassign các báo cáo trước.");
            }
        }

        if (!request.IsActive
            && user.Role.Name == "Admin")
        {
            var otherActiveAdminExists =
                await _dbContext.Users
                    .AsNoTracking()
                    .AnyAsync(
                        item =>
                            item.Id != user.Id
                            && item.IsActive
                            && item.Role.Name == "Admin",
                        cancellationToken);

            if (!otherActiveAdminExists)
            {
                throw new ConflictException(
                    "Không thể khóa Admin hoạt động cuối cùng của hệ thống.");
            }
        }

        var oldIsActive = user.IsActive;
        var currentTime = DateTime.UtcNow;

        if (!request.IsActive)
        {
            var activeRefreshTokens =
                await _dbContext.RefreshTokens
                    .Where(token =>
                        token.UserId == user.Id
                        && token.RevokedAt == null
                        && token.ExpiresAt > currentTime)
                    .ToListAsync(cancellationToken);

            foreach (var token in activeRefreshTokens)
            {
                token.RevokedAt = currentTime;
            }
        }

        user.IsActive = request.IsActive;
        user.UpdatedAt = currentTime;

        _auditLogService.Add(
            userId: adminId,
            action: AuditActions.UserStatusChanged,
            entityType: AuditEntityTypes.User,
            entityId: user.Id.ToString(),
            detail: new
            {
                user.FullName,
                user.Email,
                Role = user.Role.Name,
                OldIsActive = oldIsActive,
                NewIsActive = user.IsActive,
                Reason = request.Reason.Trim()
            });

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new UserStatusActionResult(
            Id: user.Id,
            FullName: user.FullName,
            Email: user.Email,
            RoleName: user.Role.Name,
            IsActive: user.IsActive,
            UpdatedAt: user.UpdatedAt);
    }
}
