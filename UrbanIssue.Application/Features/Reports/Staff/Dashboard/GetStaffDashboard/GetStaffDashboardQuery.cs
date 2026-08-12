using MediatR;
using UrbanIssue.Application.Features.Reports.Staff.Dashboard.Common;

namespace UrbanIssue.Application.Features.Reports.Staff.Dashboard.GetStaffDashboard;

public sealed record GetStaffDashboardQuery(
    DateTime? From,
    DateTime? To)
    : IRequest<StaffDashboardResult>;
