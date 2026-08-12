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
using UrbanIssue.Application.Features.Reports.PostResolution.Common;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.PostResolution.SubmitComplaint;

public sealed class SubmitComplaintCommandHandler
    : IRequestHandler<SubmitComplaintCommand, PostResolutionActionResult>
{
    private const int ComplaintPeriodInDays = 7;
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditLogService _auditLogService;
    private readonly INotificationService _notificationService;
    private readonly IFileStorageService _fileStorageService;

    public SubmitComplaintCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IAuditLogService auditLogService,
        INotificationService notificationService,
        IFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _auditLogService = auditLogService;
        _notificationService = notificationService;
        _fileStorageService = fileStorageService;
    }

    public async Task<PostResolutionActionResult> Handle(
        SubmitComplaintCommand request,
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

        if (report.Status != ReportStatus.Resolved || !report.ResolvedAt.HasValue)
        {
            throw new ConflictException(
                "Chỉ có thể khiếu nại báo cáo đã hoàn tất xử lý (Resolved).");
        }

        if (report.HasSubmittedComplaint
            || await _dbContext.Complaints.AnyAsync(
                complaint => complaint.ReportId == report.Id,
                cancellationToken))
        {
            throw new ConflictException(
                "Báo cáo này đã từng được gửi khiếu nại.");
        }

        var currentTime = DateTime.UtcNow;
        var complaintDeadline = report.ResolvedAt.Value
            .AddDays(ComplaintPeriodInDays);

        if (currentTime > complaintDeadline)
        {
            throw new ConflictException(
                "Đã hết thời hạn 7 ngày để gửi khiếu nại.");
        }

        var reason = request.Reason.Trim();
        var complaint = new Complaint
        {
            ReportId = report.Id,
            CitizenId = citizenId,
            Reason = reason,
            Status = ComplaintStatus.Pending,
            CreatedAt = currentTime
        };

        var storedFiles = new List<StoredFile>();
        try
        {
            foreach (var image in request.Images)
            {
                var storedFile = await _fileStorageService.SaveAsync(
                    $"uploads/complaints/{report.Id:N}",
                    image,
                    cancellationToken);
                storedFiles.Add(storedFile);
                complaint.Images.Add(new ComplaintImage
                {
                    ImageUrl = storedFile.PublicUrl,
                    UploadedAt = currentTime
                });
            }

            report.HasSubmittedComplaint = true;
            report.ComplaintSubmittedAt = currentTime;
            report.ComplaintReason = reason;
            report.UpdatedAt = currentTime;

            _dbContext.Complaints.Add(complaint);
            var statusUpdate = new StatusUpdate
            {
                ReportId = report.Id,
                UpdatedByUserId = citizenId,
                OldStatus = report.Status,
                NewStatus = report.Status,
                EventType = TimelineEventType.ComplaintSubmitted,
                Note = $"Citizen đã gửi khiếu nại: {reason}",
                CreatedAt = currentTime
            };

            foreach (var complaintImage in complaint.Images)
            {
                statusUpdate.Images.Add(new StatusUpdateImage
                {
                    ImageUrl = complaintImage.ImageUrl
                });
            }
            
            _dbContext.StatusUpdates.Add(statusUpdate);

            _auditLogService.Add(
                citizenId,
                AuditActions.ComplaintSubmitted,
                AuditEntityTypes.Report,
                report.Id.ToString(),
                new
                {
                    report.ReportCode,
                    ComplaintReason = reason,
                    ComplaintDeadline = complaintDeadline,
                    ImageCount = storedFiles.Count
                });

            var recipientIds = (await _dbContext.Users
                .AsNoTracking()
                .Where(user => user.IsActive && user.Role.Name == "Admin")
                .Select(user => user.Id)
                .ToListAsync(cancellationToken))
                .ToHashSet();

            if (report.AssignedStaffId.HasValue)
            {
                recipientIds.Add(report.AssignedStaffId.Value);
            }

            _notificationService.AddMany(
                recipientIds,
                report.Id,
                NotificationType.ComplaintSubmitted,
                "Có khiếu nại mới",
                $"Citizen đã gửi khiếu nại đối với phản ánh {report.ReportCode}.",
                currentTime);

            await _dbContext.SaveChangesAsync(cancellationToken);
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
                    // Preserve the original exception.
                }
            }

            throw;
        }

        return new PostResolutionActionResult(
            report.Id,
            report.Status,
            complaint.CreatedAt,
            complaintDeadline,
            report.ClosedAt,
            report.ReopenedAt,
            report.DueAt,
            report.ReportCode,
            complaint.Id,
            complaint.Status);
    }
}
