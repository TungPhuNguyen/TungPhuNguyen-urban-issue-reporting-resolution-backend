using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Constants;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Auditing;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Users.Admin.Common;
using UrbanIssue.Domain.Entities;

namespace UrbanIssue.Application.Features.Users.Admin.CreateStaff;

public sealed class CreateStaffCommandHandler
    : IRequestHandler<
        CreateStaffCommand,
        StaffActionResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuditLogService _auditLogService;

    public CreateStaffCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IPasswordHasher passwordHasher,
        IAuditLogService auditLogService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _passwordHasher = passwordHasher;
        _auditLogService = auditLogService;
    }

    public async Task<StaffActionResult> Handle(
        CreateStaffCommand request,
        CancellationToken cancellationToken)
    {
        var adminId = _currentUserService.UserId;

        var fullName = request.FullName.Trim();
        var normalizedEmail =
            request.Email.Trim().ToLowerInvariant();

        var emailExists = await _dbContext.Users
            .AsNoTracking()
            .AnyAsync(
                user => user.Email == normalizedEmail,
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

        if (department is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy phòng ban có ID {request.DepartmentId}.");
        }

        if (!department.IsActive)
        {
            throw new ConflictException(
                "Không thể tạo Staff cho phòng ban đã ngừng hoạt động.");
        }

        var staffRole = await _dbContext.Roles
            .AsNoTracking()
            .Where(role => role.Name == "Staff")
            .Select(role => new
            {
                role.Id
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (staffRole is null)
        {
            throw new ConflictException(
                "Hệ thống chưa có vai trò Staff.");
        }

        var currentTime = DateTime.UtcNow;

        var staff = new User
        {
            Id = Guid.NewGuid(),
            FullName = fullName,
            Email = normalizedEmail,

            /*
             * Dùng cùng password hasher đang được sử dụng
             * trong RegisterCommandHandler.
             */
            PasswordHash = _passwordHasher.Hash(
                request.Password),

            RoleId = staffRole.Id,
            DepartmentId = department.Id,

            IsActive = true,
            CreatedAt = currentTime,
            UpdatedAt = null
        };

        _dbContext.Users.Add(staff);

        _auditLogService.Add(
            userId: adminId,
            action: AuditActions.StaffCreated,
            entityType: AuditEntityTypes.User,
            entityId: staff.Id.ToString(),
            detail: new
            {
                staff.FullName,
                staff.Email,
                Role = "Staff",
                staff.DepartmentId,
                DepartmentName = department.Name,
                staff.IsActive
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
