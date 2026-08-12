using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanIssue.Application.Common.Interfaces.Persistence;
using UrbanIssue.Application.Features.Reports.Admin.Common;
using UrbanIssue.Application.Features.Reports.Common;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Admin.GetAdminReportById;

public sealed class GetAdminReportByIdQueryHandler
    : IRequestHandler<
        GetAdminReportByIdQuery,
        AdminReportDetailResult>
{
    private readonly IApplicationDbContext _dbContext;

    public GetAdminReportByIdQueryHandler(
        IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AdminReportDetailResult> Handle(
        GetAdminReportByIdQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _dbContext.Reports
            .AsNoTracking()
            .Where(report =>
                report.Id == request.ReportId)
            .Select(report =>
                new AdminReportDetailResult(
                    report.Id,
                    report.ReportNumber,
                    report.ReportCode,
                    report.Title,

                    report.CitizenId,
                    report.Citizen.FullName,
                    report.Citizen.Email,

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
                    report.RequiresManualAssignment,

                    report.Upvotes.Count(),
                    report.Comments.Count(),

                    report.Images
                        .OrderBy(image => image.Id)
                        .Select(image => image.ImageUrl)
                        .ToList(),

                    report.SLAConfigId,
                    report.AppliedSLAHours,
                    report.SLAStartedAt,
                    report.DueAt,

                    report.IsEscalated,
                    report.EscalatedAt,

                    report.CreatedAt,
                    report.UpdatedAt,
                    report.AcceptedAt,
                    report.ResolvedAt,
                    report.ClosedAt,

                    report.HasSubmittedComplaint,
                    report.ComplaintSubmittedAt,
                    report.ComplaintReason,

                    report.RejectedAt,
                    report.RejectedReason,

                    report.ReopenedAt,
                    report.ReopenReason,

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
                        report.Status == ReportStatus.New
                            && !report.Category.IsOther,
                        report.Status == ReportStatus.Assigned
                            || report.Status == ReportStatus.Accepted
                            || report.Status == ReportStatus.InProgress,
                        report.Category.IsOther
                            && report.Status == ReportStatus.New,
                        false,
                        false,
                        false,
                        false,
                        report.Status == ReportStatus.Resolved
                            && !report.Complaints.Any(complaint =>
                                complaint.Status == ComplaintStatus.Pending),
                        false,
                        report.Complaints.Any(complaint =>
                            complaint.Status == ComplaintStatus.Pending),
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
