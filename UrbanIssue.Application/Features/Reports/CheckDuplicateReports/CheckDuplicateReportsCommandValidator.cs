using FluentValidation;
using UrbanIssue.Application.Common.Rules;

namespace UrbanIssue.Application.Features.Reports.CheckDuplicateReports;

public sealed class CheckDuplicateReportsCommandValidator
    : AbstractValidator<CheckDuplicateReportsCommand>
{
    public CheckDuplicateReportsCommandValidator()
    {
        RuleFor(command => command.CategoryId)
            .GreaterThan(0)
            .WithMessage(
                "ID loại sự cố phải lớn hơn 0.");

        RuleFor(command => command.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage(
                "Vĩ độ phải nằm trong khoảng từ -90 đến 90.");

        RuleFor(command => command.Longitude)
            .InclusiveBetween(-180, 180)
            .WithMessage(
                "Kinh độ phải nằm trong khoảng từ -180 đến 180.");

        RuleFor(command => command)
            .Must(command => HanoiLocationRules.IsInsideHanoi(
                command.Latitude,
                command.Longitude))
            .WithName("Location")
            .WithMessage("Tọa độ phải nằm trong phạm vi Hà Nội và không được là (0,0).");
    }
}
