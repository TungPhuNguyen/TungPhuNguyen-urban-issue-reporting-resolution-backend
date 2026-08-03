using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.GetMyReportByCode;

public sealed class GetMyReportByCodeQueryValidator
    : AbstractValidator<GetMyReportByCodeQuery>
{
    public GetMyReportByCodeQueryValidator()
    {
        RuleFor(query => query.ReportCode)
            .NotEmpty()
            .Matches("^UI-[0-9]{6,}$")
            .WithMessage("Mã phản ánh không hợp lệ.");
    }
}
