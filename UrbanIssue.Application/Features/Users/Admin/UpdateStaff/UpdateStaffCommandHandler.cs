using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Constants;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Auditing;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Users.Admin.Common;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Users.Admin.UpdateStaff;

public sealed class UpdateStaffCommandHandler
    : IRequestHandler<
        UpdateStaffCommand,
        StaffActionResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLogService;

    public UpdateStaffCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IAuditLogService auditLogService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _auditLogService = auditLogService;
    }

    public async Task<StaffActionResult> Handle(
        UpdateStaffCommand request,
        CancellationToken cancellationToken)
    {
        var adminId = _currentUserService.UserId;

        var staff = await _dbContext.Users
            .Include(user => user.Role)
            .SingleOrDefaultAsync(
                user => user.Id == request.StaffId,
                cancellationToken);

        if (staff is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy Staff có ID {request.StaffId}.");
        }

        if (staff.Role.Name != "Staff")
        {
            throw new ConflictException(
                "Tài khoản được chọn không có vai trò Staff.");
        }

        var normalizedEmail =
            request.Email.Trim().ToLowerInvariant();

        var emailExists = await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(
                user =>
                    user.Email == normalizedEmail
                    && user.Id != staff.Id,
                cancellationToken);

        if (emailExists)
        {
            throw new ConflictException(
                "Email đã được sử dụng bởi tài khoản khác.");
        }

        var department = await _dbContext.Departments
            .AsNoTracking()
            .Where(item =>
                item.Id == request.DepartmentId)
            .Select(item => new
            {
                item.Id,
                item.Name,
                item.IsActive
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (staff.DepartmentId != request.DepartmentId)
        {
            var hasActiveReports = await _dbContext.Reports
                .AsNoTracking()
                .AnyAsync(
                    report =>
                        report.AssignedStaffId == staff.Id
                        && (
                            report.Status == ReportStatus.Assigned
                            || report.Status == ReportStatus.Accepted
                            || report.Status == ReportStatus.InProgress
                        ),
                    cancellationToken);

            if (hasActiveReports)
            {
                throw new ConflictException(
                    "Không thể chuyển phòng ban vì Staff đang giữ "
                    + "báo cáo chưa hoàn tất. Hãy reassign các báo cáo trước.");
            }
        }

        if (department is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy phòng ban có ID {request.DepartmentId}.");
        }

        if (!department.IsActive)
        {
            throw new ConflictException(
                "Không thể chuyển Staff vào phòng ban đã ngừng hoạt động.");
        }

        var oldFullName = staff.FullName;
        var oldEmail = staff.Email;
        var oldDepartmentId = staff.DepartmentId;

        staff.FullName = request.FullName.Trim();
        staff.Email = normalizedEmail;
        staff.DepartmentId = department.Id;
        staff.UpdatedAt = DateTime.UtcNow;

        _auditLogService.Add(
            userId: adminId,
            action: AuditActions.StaffUpdated,
            entityType: AuditEntityTypes.User,
            entityId: staff.Id.ToString(),
            detail: new
            {
                OldFullName = oldFullName,
                NewFullName = staff.FullName,

                OldEmail = oldEmail,
                NewEmail = staff.Email,

                OldDepartmentId = oldDepartmentId,
                NewDepartmentId = staff.DepartmentId,

                DepartmentName = department.Name
            });

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return new StaffActionResult(
            Id: staff.Id,
            FullName: staff.FullName,
            Email: staff.Email,
            DepartmentId: department.Id,
            DepartmentName: department.Name,
            IsActive: staff.IsActive,
            CreatedAt: staff.CreatedAt,
            UpdatedAt: staff.UpdatedAt);
    }
}
