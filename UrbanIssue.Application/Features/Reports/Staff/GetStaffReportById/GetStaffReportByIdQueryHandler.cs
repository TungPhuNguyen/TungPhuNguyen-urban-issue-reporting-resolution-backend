using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Exceptions;
using UrbanIssue.Application.Common.Interfaces.Authentication;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Reports.Staff.Common;
using UrbanIssue.Application.Features.Reports.Common;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Staff.GetStaffReportById;

public sealed class GetStaffReportByIdQueryHandler
    : IRequestHandler<
        GetStaffReportByIdQuery,
        StaffReportDetailResult>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public GetStaffReportByIdQueryHandler(
        IApplicationDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<StaffReportDetailResult> Handle(
        GetStaffReportByIdQuery request,
        CancellationToken cancellationToken)
    {
        var staffId = _currentUserService.UserId;

        var departmentId = await _dbContext.Users
            .AsNoTracking()
            .Where(user =>
                user.Id == staffId
                && user.IsActive
                && user.Role.Name == "Staff")
            .Select(user => user.DepartmentId)
            .SingleOrDefaultAsync(cancellationToken);

        if (!departmentId.HasValue)
        {
            throw new ConflictException(
                "Tài khoản Staff chưa được gán phòng ban.");
        }

        var result = await _dbContext.Reports
            .AsNoTracking()
            .Where(report =>
                report.Id == request.ReportId
                && report.DepartmentId == departmentId.Value
                && (
                    report.Status == ReportStatus.Assigned
                    || report.AssignedStaffId == staffId
                ))
            .Select(report =>
                new StaffReportDetailResult(
                    report.Id,
                    report.ReportNumber,
                    report.ReportCode,
                    report.Title,

                    report.CitizenId,
                    report.Citizen.FullName,

                    report.CategoryId,
                    report.Category.Name,

                    report.AreaId,
                    report.Area.Name,

                    report.DepartmentId,
                    report.Department == null
                        ? null
                        : report.Department.Name,

                    report.AssignedStaffId,
                    report.AssignedStaff == null
                        ? null
                        : report.AssignedStaff.FullName,

                    report.Description,
                    report.OtherCategoryText,
                    report.AddressText,
                    report.Latitude,
                    report.Longitude,

                    report.Priority,
                    report.Status,

                    report.Upvotes.Count(),

                    report.Images
                        .OrderBy(image => image.Id)
                        .Select(image => image.ImageUrl)
                        .ToList(),

                    report.AppliedSLAHours,
                    report.SLAStartedAt,
                    report.DueAt,

                    report.IsEscalated,
                    report.EscalatedAt,

                    report.HasSubmittedComplaint,
                    report.ComplaintSubmittedAt,
                    report.ComplaintReason,

                    report.CreatedAt,
                    report.UpdatedAt,
                    report.AcceptedAt,
                    report.ResolvedAt,

                    report.Complaints
                        .OrderByDescending(complaint => complaint.CreatedAt)
                        .Select(complaint => new ComplaintResult(
                            complaint.Id,
                            complaint.Status,
                            complaint.Reason,
                            complaint.AdminDecisionReason,
                            complaint.ResolvedByAdminId,
                            complaint.ResolvedByAdmin == null
                                ? null
                                : complaint.ResolvedByAdmin.FullName,
                            complaint.CreatedAt,
                            complaint.ResolvedAt,
                            complaint.Images
                                .OrderBy(image => image.Id)
                                .Select(image => image.ImageUrl)
                                .ToList()))
                        .FirstOrDefault(),

                    report.StatusUpdates
                        .Where(update => update.NewStatus == ReportStatus.Resolved)
                        .OrderByDescending(update => update.CreatedAt)
                        .Select(update => new ReportResolutionResult(
                            update.Note,
                            update.UpdatedByUserId,
                            update.UpdatedByUser == null
                                ? null
                                : update.UpdatedByUser.FullName,
                            update.CreatedAt,
                            update.Images
                                .OrderBy(image => image.Id)
                                .Select(image => image.ImageUrl)
                                .ToList()))
                        .FirstOrDefault(),

                    new ReportAllowedActionsResult(
                        false,
                        false,
                        false,
                        false,
                        false,
                        report.Status == ReportStatus.Assigned
                            && (!report.AssignedStaffId.HasValue
                                || report.AssignedStaffId == staffId),
                        report.Status == ReportStatus.Accepted
                            && report.AssignedStaffId == staffId,
                        report.Status == ReportStatus.InProgress
                            && report.AssignedStaffId == staffId,
                        report.Status == ReportStatus.InProgress
                            && report.AssignedStaffId == staffId,
                        false,
                        false,
                        false,
                        false),

                    report.RowVersion))
            .SingleOrDefaultAsync(cancellationToken);

        if (result is null)
        {
            throw new KeyNotFoundException(
                $"Không tìm thấy báo cáo có ID {request.ReportId}.");
        }

        return result;
    }
}
