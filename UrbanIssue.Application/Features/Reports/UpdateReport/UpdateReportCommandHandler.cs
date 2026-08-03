using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Constants;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Auditing;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Interfaces.Notifications;
using UrbanIssue.Application.Features.Reports.CheckDuplicateReports;
using UrbanIssue.Application.Features.Reports.Common;
using UrbanIssue.Application.Features.Reports.GetMyReportById;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.UpdateReport;

public sealed class UpdateReportCommandHandler
    : IRequestHandler<UpdateReportCommand, CitizenReportDetailResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IReportDuplicateChecker _duplicateChecker;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;
    private readonly ISender _sender;

    public UpdateReportCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IReportDuplicateChecker duplicateChecker,
        IAuditLogService auditLogService,
        INotificationService notificationService,
        ISender sender)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _duplicateChecker = duplicateChecker;
        _auditLogService = auditLogService;
        _notificationService = notificationService;
        _sender = sender;
    }

    public async Task<CitizenReportDetailResult> Handle(
        UpdateReportCommand request,
        CancellationToken cancellationToken)
    {
        var citizenId = _currentUserService.UserId;
        var report = await _dbContext.Reports.SingleOrDefaultAsync(
            item => item.Id == request.ReportId && item.CitizenId == citizenId,
            cancellationToken);

        if (report is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy báo cáo có ID {request.ReportId}.");
        }

        if (report.Status is not (ReportStatus.New or ReportStatus.Assigned))
        {
            throw new ConflictException(
                "Chỉ được sửa phản ánh trước khi Staff tiếp nhận.");
        }

        if (!report.RowVersion.SequenceEqual(request.RowVersion))
        {
            throw new ConflictException(
                "Phản ánh đã được cập nhật. Hãy tải lại dữ liệu trước khi sửa.");
        }

        var category = await _dbContext.Categories.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == request.CategoryId, cancellationToken);
        var area = await _dbContext.Areas.AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == request.AreaId, cancellationToken);

        if (category is null || !category.IsActive)
        {
            throw new KeyNotFoundException("Loại sự cố không tồn tại hoặc đã ngừng hoạt động.");
        }

        if (area is null || !area.IsActive || !area.ParentAreaId.HasValue)
        {
            throw new KeyNotFoundException("Phường/xã không tồn tại hoặc đã ngừng hoạt động.");
        }

        if (category.IsOther && string.IsNullOrWhiteSpace(request.OtherCategoryText))
        {
            throw new ConflictException(
                "Khi chọn loại sự cố 'Khác', bạn phải mô tả loại sự cố cụ thể.");
        }

        if (!category.IsOther && !request.ConfirmPossibleDuplicate)
        {
            var duplicates = await _duplicateChecker.CheckAsync(
                request.CategoryId,
                request.Latitude,
                request.Longitude,
                cancellationToken,
                report.Id);
            if (duplicates.HasPossibleDuplicates)
            {
                throw new PotentialDuplicateException(duplicates);
            }
        }

        var routingCandidates = new List<RoutingCandidate>();
        if (!category.IsOther)
        {
            routingCandidates = await _dbContext.RoutingRules.AsNoTracking()
                .Where(rule => rule.IsActive
                    && rule.CategoryId == request.CategoryId
                    && rule.AreaId == request.AreaId
                    && rule.Department.IsActive)
                .OrderBy(rule => rule.PriorityOrder)
                .ThenBy(rule => rule.Id)
                .Select(rule => new RoutingCandidate(
                    rule.DepartmentId,
                    rule.PriorityOrder))
                .ToListAsync(cancellationToken);
        }

        var selectedDepartmentId = routingCandidates.Count == 0
            ? (int?)null
            : routingCandidates
                .Where(candidate => candidate.PriorityOrder
                    == routingCandidates[0].PriorityOrder)
                .Select(candidate => candidate.DepartmentId)
                .Distinct()
                .Take(2)
                .ToList() is { Count: 1 } departments
                    ? departments[0]
                    : null;

        var oldCategoryId = report.CategoryId;
        var oldAreaId = report.AreaId;
        var oldStatus = report.Status;
        var oldAssignedStaffId = report.AssignedStaffId;
        var currentTime = DateTime.UtcNow;

        report.CategoryId = request.CategoryId;
        report.AreaId = request.AreaId;
        report.Title = request.Title.Trim();
        report.Description = request.Description.Trim();
        report.OtherCategoryText = category.IsOther
            ? request.OtherCategoryText!.Trim()
            : null;
        report.AddressText = string.IsNullOrWhiteSpace(request.AddressText)
            ? null
            : request.AddressText.Trim();
        report.Latitude = request.Latitude;
        report.Longitude = request.Longitude;
        report.DepartmentId = selectedDepartmentId;
        report.AssignedStaffId = null;
        report.Status = selectedDepartmentId.HasValue
            ? ReportStatus.Assigned
            : ReportStatus.New;
        report.RequiresManualAssignment = !selectedDepartmentId.HasValue;
        report.UpdatedAt = currentTime;

        _dbContext.StatusUpdates.Add(new StatusUpdate
        {
            ReportId = report.Id,
            UpdatedByUserId = citizenId,
            OldStatus = oldStatus,
            NewStatus = report.Status,
            EventType = TimelineEventType.ReportEdited,
            Note = "Citizen đã chỉnh sửa nội dung/vị trí phản ánh.",
            CreatedAt = currentTime
        });

        _auditLogService.Add(
            citizenId,
            AuditActions.ReportEdited,
            AuditEntityTypes.Report,
            report.Id.ToString(),
            new
            {
                report.ReportCode,
                OldCategoryId = oldCategoryId,
                NewCategoryId = report.CategoryId,
                OldAreaId = oldAreaId,
                NewAreaId = report.AreaId,
                OldStatus = oldStatus,
                NewStatus = report.Status
            });

        if (oldAssignedStaffId.HasValue)
        {
            _notificationService.Add(
                oldAssignedStaffId.Value,
                report.Id,
                NotificationType.ReportReassigned,
                "Citizen đã chỉnh sửa phản ánh",
                $"Phản ánh {report.ReportCode} đã được chỉnh sửa và cần phân công lại.",
                currentTime);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await _sender.Send(
            new GetMyReportByIdQuery(report.Id),
            cancellationToken);
    }

    private sealed record RoutingCandidate(
        int DepartmentId,
        int PriorityOrder);
}
