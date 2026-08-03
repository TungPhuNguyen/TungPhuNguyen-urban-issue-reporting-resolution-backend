using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using UrbanIssue.Application.Common.Constants;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Auditing;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Geography;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Common.Interfaces.Storage;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Common.Settings;
using UrbanIssue.Application.Features.Reports.CheckDuplicateReports;
using UrbanIssue.Domain.Entities;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.CreateReport;

public sealed class CreateReportCommandHandler
    : IRequestHandler<
        CreateReportCommand,
        CreateReportResult>
{
    private readonly IApplicationDbContext
        _dbContext;

    private readonly ICurrentUserService
        _currentUserService;

    private readonly IFileStorageService
        _fileStorageService;
    private readonly IAuditLogService _auditLogService;
    private readonly IReportDuplicateChecker _duplicateChecker;
    private readonly IAreaBoundaryService _areaBoundaryService;
    private readonly LocationValidationSettings _locationSettings;

    public CreateReportCommandHandler(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IFileStorageService fileStorageService,
    IAuditLogService auditLogService,
    IReportDuplicateChecker duplicateChecker,
    IAreaBoundaryService areaBoundaryService,
    IOptions<LocationValidationSettings> locationSettings)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _fileStorageService = fileStorageService;
        _auditLogService = auditLogService;
        _duplicateChecker = duplicateChecker;
        _areaBoundaryService = areaBoundaryService;
        _locationSettings = locationSettings.Value;
    }
    public async Task<CreateReportResult> Handle(
        CreateReportCommand request,
        CancellationToken cancellationToken)
    {
        var category =
            await _dbContext.Categories
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    category =>
                        category.Id == request.CategoryId,
                    cancellationToken);

        if (category is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy loại sự cố có ID {request.CategoryId}.");
        }

        if (!category.IsActive)
        {
            throw new ConflictException(
                "Không thể tạo báo cáo với loại sự cố đã ngừng hoạt động.");
        } 

        var area =
            await _dbContext.Areas
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    area =>
                        area.Id == request.AreaId,
                    cancellationToken);

        if (area is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy khu vực có ID {request.AreaId}.");
        }

        if (!area.IsActive)
        {
            throw new ConflictException(
                "Không thể tạo báo cáo tại khu vực đã ngừng hoạt động.");
        }

        /*
 * Report phải chọn Phường.
 * Area cấp Quận chỉ dùng để nhóm các Phường.
 */
        if (!area.ParentAreaId.HasValue)
        {
            throw new ConflictException(
                "Vui lòng chọn Phường. "
                + "Không thể gửi báo cáo trực tiếp cho Quận.");
        }

        var boundaryCheck = await _areaBoundaryService.CheckAsync(
            request.AreaId,
            request.Latitude,
            request.Longitude,
            cancellationToken);

        if (boundaryCheck.HasBoundary && !boundaryCheck.ContainsPoint)
        {
            throw new ConflictException(
                "Tọa độ không nằm trong ranh giới phường/xã đã chọn.");
        }

        if (!boundaryCheck.HasBoundary
            && _locationSettings.RequireWardBoundary)
        {
            throw new ConflictException(
                "Phường/xã đã chọn chưa có dữ liệu polygon để xác minh tọa độ.");
        }

        if (category.IsOther
            && string.IsNullOrWhiteSpace(request.OtherCategoryText))
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
                cancellationToken);

            if (duplicates.HasPossibleDuplicates)
            {
                throw new PotentialDuplicateException(duplicates);
            }
        }

        /*
         * Tìm các RoutingRule đang hoạt động
         * khớp chính xác Category + Area.
         */
        var routingCandidates = new List<RoutingCandidate>();

        if (!category.IsOther)
        {
            routingCandidates = await _dbContext.RoutingRules
                .AsNoTracking()
                .Where(rule =>
                    rule.IsActive
                    && rule.CategoryId
                        == request.CategoryId
                    && rule.AreaId
                        == request.AreaId
                    && rule.Department.IsActive)
                .OrderBy(rule =>
                    rule.PriorityOrder)
                .ThenBy(rule =>
                    rule.Id)
                .Select(rule =>
                    new RoutingCandidate(
                        rule.DepartmentId,
                        rule.Department.Name,
                        rule.PriorityOrder))
                .ToListAsync(
                    cancellationToken);
        }

        int? departmentId = null;
        string? departmentName = null;

        var requiresManualAssignment =
            true;

        var reportStatus =
            ReportStatus.New;

        if (routingCandidates.Count > 0)
        {
            var bestPriorityOrder =
                routingCandidates[0]
                    .PriorityOrder;

            var bestCandidates =
                routingCandidates
                    .Where(candidate =>
                        candidate.PriorityOrder
                            == bestPriorityOrder)
                    .GroupBy(candidate =>
                        candidate.DepartmentId)
                    .Select(group =>
                        group.First())
                    .ToList();

            /*
             * Chỉ tự động phân công khi có đúng
             * một Department ở mức ưu tiên tốt nhất.
             */
            if (bestCandidates.Count == 1)
            {
                var selectedCandidate =
                    bestCandidates[0];

                departmentId =
                    selectedCandidate.DepartmentId;

                departmentName =
                    selectedCandidate.DepartmentName;

                requiresManualAssignment =
                    false;

                reportStatus =
                    ReportStatus.Assigned;
            }
        }

        var currentTime =
            DateTime.UtcNow;

        var report =
            new Report
            {
                Id =
                    Guid.NewGuid(),

                CitizenId = _currentUserService.UserId,

                CategoryId =
                    request.CategoryId,

                AreaId =
                    request.AreaId,

                DepartmentId =
                    departmentId,

                Title =
                    request.Title.Trim(),

                Description =
                    request.Description.Trim(),

                OtherCategoryText =
                    category.IsOther
                        ? request.OtherCategoryText!.Trim()
                        : null,

                AddressText =
                    string.IsNullOrWhiteSpace(
                        request.AddressText)
                        ? null
                        : request.AddressText.Trim(),

                Latitude =
                    request.Latitude,

                Longitude =
                    request.Longitude,

                Status =
                    reportStatus,

                RequiresManualAssignment =
                    requiresManualAssignment,

                CreatedAt =
                    currentTime,

                UpdatedAt =
                    null
            };
        var initialStatusNote =
    reportStatus == ReportStatus.Assigned
        ? $"Báo cáo được tạo và tự động phân công đến {departmentName}."
        : "Báo cáo được tạo và đang chờ Admin phân công đơn vị xử lý.";

        var initialStatusUpdate =
            new StatusUpdate
            {
                ReportId =
                    report.Id,

                UpdatedByUserId =
                    _currentUserService.UserId,

                OldStatus =
                    reportStatus,

                NewStatus =
                    reportStatus,

                EventType =
                    TimelineEventType.ReportCreated,

                Note =
                    initialStatusNote,

                CreatedAt =
                    currentTime
            };
        var storedFiles =
            new List<StoredFile>();

        try
        {
            foreach (var image in request.Images)
            {
                var storedFile =
                    await _fileStorageService
                        .SaveAsync(
                            folder:
                                $"uploads/reports/{report.Id:N}",
                            file:
                                image,
                            cancellationToken);

                storedFiles.Add(
                    storedFile);
            }

            var reportImages =
    storedFiles
        .Select(storedFile =>
            new ReportImage
            {
                ReportId =
                    report.Id,

                ImageUrl =
                    storedFile.PublicUrl,

                UploadedAt =
                    currentTime
            })
        .ToList();

            _dbContext.Reports.Add(
                report);

            _dbContext.ReportImages.AddRange(
                reportImages);
            
            _dbContext.StatusUpdates.Add(
                initialStatusUpdate);

            _auditLogService.Add(
    userId:
        report.CitizenId,

    action:
        AuditActions.ReportCreated,

    entityType:
        AuditEntityTypes.Report,

    entityId:
        report.Id.ToString(),

    detail:
        new
        {
            report.ReportCode,
            report.Title,
            report.CategoryId,
            report.AreaId,
            report.DepartmentId,
            report.Status,
            report.RequiresManualAssignment,
            report.Latitude,
            report.Longitude,
            ImageCount = reportImages.Count
        });

            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }
        catch
        {
            foreach (var storedFile in storedFiles)
            {
                try
                {
                    await _fileStorageService
                        .DeleteAsync(
                            storedFile.StorageKey,
                            CancellationToken.None);
                }
                catch
                {
                    // Không che mất exception ban đầu.
                }
            }

            throw;
        }

        return new CreateReportResult(
            Id:
                report.Id,

            ReportNumber:
                report.ReportNumber,

            ReportCode:
                report.ReportCode,

            Title:
                report.Title,

            Status:
                report.Status,

            DepartmentId:
                report.DepartmentId,

            DepartmentName:
                departmentName,

            RequiresManualAssignment:
                report.RequiresManualAssignment,

            CreatedAt:
                report.CreatedAt,

            ImageUrls:
                storedFiles
                    .Select(file =>
                        file.PublicUrl)
                    .ToList());

    }

    private sealed record RoutingCandidate(
        int DepartmentId,
        string DepartmentName,
        int PriorityOrder);
}
