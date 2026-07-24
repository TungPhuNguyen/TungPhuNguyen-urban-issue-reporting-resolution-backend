using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.Public.GetPublicReportById;

public sealed class GetPublicReportByIdQueryValidator
    : AbstractValidator<GetPublicReportByIdQuery>
{
    public GetPublicReportByIdQueryValidator()
    {
        RuleFor(x => x.ReportId)
            .NotEqual(Guid.Empty)
            .WithMessage(
                "ID báo cáo không hợp lệ.");
    }
}
