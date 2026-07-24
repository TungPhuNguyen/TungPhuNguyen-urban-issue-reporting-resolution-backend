using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Authentication;
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
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IFileStorageService _fileStorageService;

    public ResolveReportCommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService,
        IFileStorageService fileStorageService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
    }

    public async Task<StaffReportActionResult> Handle(
        ResolveReportCommand request,
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
                "Không tìm thấy báo cáo hoặc báo cáo không thuộc Staff hiện tại.");
        }

        if (report.Status != ReportStatus.InProgress)
        {
            throw new ConflictException(
                "Chỉ có thể hoàn tất báo cáo đang được xử lý.");
        }

        var storedFiles = new List<StoredFile>();

        try
        {
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

            report.Status = ReportStatus.Resolved;
            report.ResolvedAt = currentTime;
            report.UpdatedAt = currentTime;

            var statusUpdate =
                new StatusUpdate
                {
                    ReportId = report.Id,
                    UpdatedByUserId = staffId,
                    OldStatus = oldStatus,
                    NewStatus = ReportStatus.Resolved,
                    Note = request.Note.Trim(),
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
            foreach (var storedFile in storedFiles)
            {
                await _fileStorageService.DeleteAsync(
                    storedFile.StorageKey,
                    cancellationToken);
            }

            throw;
        }
    }
}
