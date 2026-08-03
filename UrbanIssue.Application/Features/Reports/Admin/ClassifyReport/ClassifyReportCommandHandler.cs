using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Constants;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Auditing;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Notifications;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Reports.Admin.Common;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Admin.ClassifyReport;

public sealed class ClassifyReportCommandHandler
    : IRequestHandler<ClassifyReportCommand, AdminReportActionResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;

    public ClassifyReportCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IAuditLogService auditLogService,
        INotificationService notificationService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _auditLogService = auditLogService;
        _notificationService = notificationService;
    }

    public async Task<AdminReportActionResult> Handle(
        ClassifyReportCommand request,
        CancellationToken cancellationToken)
    {
        var report = await _dbContext.Reports
            .Include(item => item.Category)
            .SingleOrDefaultAsync(item => item.Id == request.ReportId, cancellationToken);

        if (report is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy báo cáo có ID {request.ReportId}.");
        }

        if (!report.Category.IsOther || report.Status != ReportStatus.New)
        {
            throw new ConflictException(
                "Chỉ có thể phân loại báo cáo 'Khác' đang chờ Admin xử lý.");
        }

        var category = await _dbContext.Categories
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == request.CategoryId,
                cancellationToken);

        if (category is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy loại sự cố có ID {request.CategoryId}.");
        }

        if (!category.IsActive || category.IsOther)
        {
            throw new ConflictException(
                "Loại sự cố sau phân loại phải là danh mục cụ thể đang hoạt động.");
        }

        var bestPriority = await _dbContext.RoutingRules
            .AsNoTracking()
            .Where(rule => rule.IsActive
                && rule.CategoryId == category.Id
                && rule.AreaId == report.AreaId
                && rule.Department.IsActive)
            .MinAsync(rule => (int?)rule.PriorityOrder, cancellationToken);

        var candidates = new List<RoutingCandidate>();
        if (bestPriority.HasValue)
        {
            candidates = await _dbContext.RoutingRules
                .AsNoTracking()
                .Where(rule => rule.IsActive
                    && rule.CategoryId == category.Id
                    && rule.AreaId == report.AreaId
                    && rule.PriorityOrder == bestPriority.Value
                    && rule.Department.IsActive)
                .Select(rule => new RoutingCandidate(
                    rule.DepartmentId,
                    rule.Department.Name))
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        var oldCategoryId = report.CategoryId;
        var oldCategoryName = report.Category.Name;
        var currentTime = DateTime.UtcNow;

        report.CategoryId = category.Id;
        report.DepartmentId = candidates.Count == 1
            ? candidates[0].DepartmentId
            : null;
        report.Status = candidates.Count == 1
            ? ReportStatus.Assigned
            : ReportStatus.New;
        report.RequiresManualAssignment = candidates.Count != 1;
        report.UpdatedAt = currentTime;

        var note = string.IsNullOrWhiteSpace(request.Note)
            ? $"Admin phân loại từ '{oldCategoryName}' sang '{category.Name}'."
            : request.Note.Trim();

        _dbContext.StatusUpdates.Add(new StatusUpdate
        {
            ReportId = report.Id,
            UpdatedByUserId = _currentUserService.UserId,
            OldStatus = ReportStatus.New,
            NewStatus = report.Status,
            EventType = TimelineEventType.CategoryChanged,
            Note = note,
            CreatedAt = currentTime
        });

        _auditLogService.Add(
            _currentUserService.UserId,
            AuditActions.ReportReclassified,
            AuditEntityTypes.Report,
            report.Id.ToString(),
            new
            {
                report.ReportCode,
                OldCategoryId = oldCategoryId,
                NewCategoryId = category.Id,
                report.DepartmentId,
                report.Status,
                Note = note
            });

        _notificationService.Add(
            report.CitizenId,
            report.Id,
            NotificationType.ReportReclassified,
            "Phản ánh đã được phân loại",
            $"Phản ánh {report.ReportCode} đã được chuyển sang loại '{category.Name}'.",
            currentTime);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AdminReportActionResult(
            report.Id,
            report.ReportCode,
            report.Status,
            report.DepartmentId,
            candidates.Count == 1 ? candidates[0].Name : null,
            report.AssignedStaffId,
            null,
            report.Priority,
            report.RequiresManualAssignment,
            report.UpdatedAt);
    }

    private sealed record RoutingCandidate(
        int DepartmentId,
        string Name);
}
