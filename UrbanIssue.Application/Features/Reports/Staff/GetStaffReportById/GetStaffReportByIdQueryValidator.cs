using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.Staff.GetStaffReportById;

public sealed class GetStaffReportByIdQueryValidator
    : AbstractValidator<GetStaffReportByIdQuery>
{
    public GetStaffReportByIdQueryValidator()
    {
        RuleFor(x => x.ReportId)
            .NotEqual(Guid.Empty)
            .WithMessage("ID báo cáo không hợp lệ.");
    }
}
