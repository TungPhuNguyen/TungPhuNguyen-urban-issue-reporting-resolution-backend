using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.Admin.GetAdminReportById;

public sealed class GetAdminReportByIdQueryValidator
    : AbstractValidator<GetAdminReportByIdQuery>
{
    public GetAdminReportByIdQueryValidator()
    {
        RuleFor(x => x.ReportId)
            .NotEqual(Guid.Empty)
            .WithMessage("ID báo cáo không hợp lệ.");
    }
}
