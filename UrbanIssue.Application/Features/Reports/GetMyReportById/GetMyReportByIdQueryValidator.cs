using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.GetMyReportById;

public sealed class GetMyReportByIdQueryValidator
    : AbstractValidator<GetMyReportByIdQuery>
{
    public GetMyReportByIdQueryValidator()
    {
        RuleFor(query => query.Id)
            .NotEqual(Guid.Empty)
            .WithMessage(
                "ID báo cáo không hợp lệ.");
    }
}
