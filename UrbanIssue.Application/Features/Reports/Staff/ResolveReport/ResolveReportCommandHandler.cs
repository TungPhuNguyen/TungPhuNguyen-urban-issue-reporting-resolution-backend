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

namespace UrbanIssue.Application.Features.Reports.Staff.ResolveReport;

public sealed class ResolveReportCommandHandler
    : IRequestHandler<
        ResolveReportCommand,
        StaffReportActionResult>
{
    private const int ComplaintPeriodInDays = 7;

    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;

    public ResolveReportCommandHandler(
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

    public async Task<StaffReportActionResult> Handle(
        ResolveReportCommand request,
        CancellationToken cancellationToken)
    {
        var staffId = _currentUserService.UserId;

        /*
         * Chỉ Staff đang được phân công mới có thể
         * hoàn tất Report.
         */
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
                "Chỉ có thể hoàn tất báo cáo đang được xử lý.");
        }

        var storedFiles = new List<StoredFile>();

        try
        {
            /*
             * Lưu các ảnh kết quả xử lý trước.
             * Nếu database lưu thất bại, ảnh sẽ được xóa lại.
             */
            foreach (var image in request.Images)
            {
                var storedFile =
                    await _fileStorageService.SaveAsync(
                        $"reports/{report.Id:N}",
                        image,
                        cancellationToken);

                storedFiles.Add(storedFile);
            }

            var currentTime = DateTime.UtcNow;
            var oldStatus = report.Status;

            var note = string.IsNullOrWhiteSpace(request.Note)
                ? "Staff đã hoàn tất xử lý báo cáo."
                : request.Note.Trim();

            report.Status = ReportStatus.Resolved;
            report.ResolvedAt = currentTime;
            report.ClosedAt = null;
            report.UpdatedAt = currentTime;

            /*
             * Khi Report được giải quyết lại sau Reopen,
             * bảo đảm không còn khiếu nại đang chờ.
             */
            report.ComplaintSubmittedAt = null;
            report.ComplaintReason = null;

            var statusUpdate = new StatusUpdate
            {
                ReportId = report.Id,
                UpdatedByUserId = staffId,
                OldStatus = oldStatus,
                NewStatus = ReportStatus.Resolved,
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

            /*
             * Ghi Audit Log cho thao tác Resolve.
             */
            _auditLogService.Add(
                userId: staffId,
                action: AuditActions.ReportResolved,
                entityType: AuditEntityTypes.Report,
                entityId: report.Id.ToString(),
                detail: new
                {
                    OldStatus = oldStatus,
                    NewStatus = report.Status,

                    report.AssignedStaffId,
                    report.DepartmentId,
                    report.Priority,

                    report.SLAStartedAt,
                    report.AppliedSLAHours,
                    report.DueAt,
                    report.ResolvedAt,

                    WasResolvedOnTime =
                        report.DueAt.HasValue
                        && report.ResolvedAt.Value
                            <= report.DueAt.Value,

                    Note = note,
                    ImageCount = storedFiles.Count
                });

            var complaintDeadline =
                currentTime.AddDays(
                    ComplaintPeriodInDays);

            /*
             * Thông báo cho Citizen biết Report đã được xử lý
             * và thời hạn gửi khiếu nại.
             */
            _notificationService.Add(
                userId: report.CitizenId,
                reportId: report.Id,
                type: NotificationType.ReportResolved,
                title: "Báo cáo đã được giải quyết",
                message:
                    "Báo cáo của bạn đã được đánh dấu hoàn tất. "
                    + "Bạn có thể gửi khiếu nại trước "
                    + $"{complaintDeadline:dd/MM/yyyy HH:mm} UTC.",
                createdAt: currentTime);

            /*
             * Report, StatusUpdate, StatusUpdateImage,
             * AuditLog và Notification được lưu chung
             * trong một lần SaveChangesAsync.
             */
            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return new StaffReportActionResult(
                Id: report.Id,
                Status: report.Status,
                Priority: report.Priority,
                AssignedStaffId: report.AssignedStaffId,
                SlaConfigId: report.SLAConfigId,
                AppliedSlaHours: report.AppliedSLAHours,
                SlaStartedAt: report.SLAStartedAt,
                DueAt: report.DueAt,
                UpdatedAt: report.UpdatedAt);
        }
        catch
        {
            /*
             * Xóa các file đã lưu nếu thao tác nghiệp vụ
             * hoặc SaveChangesAsync thất bại.
             *
             * Cleanup dùng CancellationToken.None để vẫn chạy
             * khi request ban đầu đã bị hủy.
             */
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
                    /*
                     * Không để lỗi cleanup che mất
                     * exception nghiệp vụ ban đầu.
                     */
                }
            }

            throw;
        }
    }
}
