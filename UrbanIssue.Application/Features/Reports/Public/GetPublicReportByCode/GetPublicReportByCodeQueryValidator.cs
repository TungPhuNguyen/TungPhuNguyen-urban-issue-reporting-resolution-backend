using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.Public.GetPublicReportByCode;

public sealed class GetPublicReportByCodeQueryValidator
    : AbstractValidator<GetPublicReportByCodeQuery>
{
    public GetPublicReportByCodeQueryValidator()
    {
        RuleFor(query => query.ReportCode)
            .NotEmpty()
            .Matches("^UI-[0-9]{6,}$")
            .WithMessage("Mã phản ánh không hợp lệ.");
    }
}
