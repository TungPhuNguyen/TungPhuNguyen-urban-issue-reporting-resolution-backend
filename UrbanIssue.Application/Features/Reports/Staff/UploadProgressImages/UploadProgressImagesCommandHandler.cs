using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Constants;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Auditing;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Notifications;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Interfaces.Storage;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Reports.Staff.Common;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Staff.UploadProgressImages;

public sealed class UploadProgressImagesCommandHandler
    : IRequestHandler<
        UploadProgressImagesCommand,
        StaffProgressUpdateResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;

    public UploadProgressImagesCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService,
        IAuditLogService auditLogService,
        INotificationService notificationService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
        _auditLogService = auditLogService;
        _notificationService = notificationService;
    }

    public async Task<StaffProgressUpdateResult> Handle(
        UploadProgressImagesCommand request,
        CancellationToken cancellationToken)
    {
        var staffId = _currentUserService.UserId;

        var report = await _dbContext.Reports
            .SingleOrDefaultAsync(
                item =>
                    item.Id == request.ReportId
                    && item.AssignedStaffId == staffId,
                cancellationToken);

        if (report is null)
        {
            throw new KeyNotFoundException(
                "Không tìm thấy báo cáo hoặc báo cáo "
                + "không thuộc Staff hiện tại.");
        }

        if (report.Status != ReportStatus.InProgress)
        {
            throw new ConflictException(
                "Chỉ có thể tải ảnh tiến độ cho báo cáo "
                + "đang ở trạng thái InProgress.");
        }

        var storedFiles = new List<StoredFile>();

        try
        {
            foreach (var image in request.Images)
            {
                var storedFile =
                    await _fileStorageService.SaveAsync(
                        $"reports/{report.Id:N}/progress",
                        image,
                        cancellationToken);

                storedFiles.Add(storedFile);
            }

            var currentTime = DateTime.UtcNow;

            var note = string.IsNullOrWhiteSpace(request.Note)
                ? "Staff đã cập nhật ảnh tiến độ xử lý."
                : request.Note.Trim();

            report.UpdatedAt = currentTime;

            var statusUpdate = new StatusUpdate
            {
                ReportId = report.Id,
                UpdatedByUserId = staffId,
                OldStatus = report.Status,
                NewStatus = report.Status,
                EventType = TimelineEventType.ProgressImagesUploaded,
                Note = note,
                CreatedAt = currentTime
            };

            foreach (var storedFile in storedFiles)
            {
                statusUpdate.Images.Add(
                    new StatusUpdateImage
                    {
                        ImageUrl = storedFile.PublicUrl
                    });
            }

            _dbContext.StatusUpdates.Add(statusUpdate);

            _auditLogService.Add(
                userId: staffId,
                action: AuditActions.ReportProgressImagesUploaded,
                entityType: AuditEntityTypes.Report,
                entityId: report.Id.ToString(),
                detail: new
                {
                    report.Status,
                    report.DepartmentId,
                    report.AssignedStaffId,
                    Note = note,
                    ImageCount = storedFiles.Count,
                    CreatedAt = currentTime
                });

            _notificationService.Add(
                userId: report.CitizenId,
                reportId: report.Id,
                type: NotificationType.ReportStatusChanged,
                title: "Có ảnh tiến độ mới",
                message:
                    "Nhân viên phụ trách đã cập nhật ảnh "
                    + "tiến độ xử lý báo cáo của bạn.",
                createdAt: currentTime);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return new StaffProgressUpdateResult(
                ReportId: report.Id,
                ReportCode: report.ReportCode,
                StatusUpdateId: statusUpdate.Id,
                Status: report.Status,
                Note: statusUpdate.Note,
                ImageUrls: storedFiles
                    .Select(file => file.PublicUrl)
                    .ToList(),
                CreatedAt: statusUpdate.CreatedAt,
                UpdatedAt: report.UpdatedAt);
        }
        catch
        {
            foreach (var storedFile in storedFiles)
            {
                try
                {
                    await _fileStorageService.DeleteAsync(
                        storedFile.StorageKey,
                        CancellationToken.None);
                }
                catch
                {
                    // Không để lỗi cleanup che mất lỗi ban đầu.
                }
            }

            throw;
        }
    }
}
