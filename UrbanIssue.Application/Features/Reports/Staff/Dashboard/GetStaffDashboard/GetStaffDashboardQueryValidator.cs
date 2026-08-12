using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.Staff.Dashboard.GetStaffDashboard;

public sealed class GetStaffDashboardQueryValidator
    : AbstractValidator<GetStaffDashboardQuery>
{
    private const int MaximumRangeInDays = 366;

    public GetStaffDashboardQueryValidator()
    {
        RuleFor(query => query)
            .Must(query =>
                !query.From.HasValue
                || !query.To.HasValue
                || query.From.Value <= query.To.Value)
            .WithMessage(
                "Thời điểm From không được lớn hơn To.");

        RuleFor(query => query)
            .Must(query =>
                !query.From.HasValue
                || !query.To.HasValue
                || (
                    query.To.Value
                    - query.From.Value
                ).TotalDays <= MaximumRangeInDays)
            .WithMessage(
                "Khoảng thời gian thống kê "
                + "không được vượt quá 366 ngày.");
    }
}
