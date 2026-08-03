using FluentValidation;
using UrbanIssue.Application.Common.Rules;

namespace UrbanIssue.Application.Features.Reports.UpdateReport;

public sealed class UpdateReportCommandValidator
    : AbstractValidator<UpdateReportCommand>
{
    public UpdateReportCommandValidator()
    {
        RuleFor(command => command.ReportId).NotEqual(Guid.Empty);
        RuleFor(command => command.CategoryId).GreaterThan(0);
        RuleFor(command => command.AreaId).GreaterThan(0);
        RuleFor(command => command.Title)
            .NotEmpty().MinimumLength(10).MaximumLength(150);
        RuleFor(command => command.Description)
            .NotEmpty().MinimumLength(10).MaximumLength(2000);
        RuleFor(command => command.OtherCategoryText).MaximumLength(250);
        RuleFor(command => command.AddressText).MaximumLength(500);
        RuleFor(command => command.RowVersion)
            .NotEmpty()
            .WithMessage("RowVersion là bắt buộc để tránh ghi đè dữ liệu mới hơn.");
        RuleFor(command => command)
            .Must(command => HanoiLocationRules.IsInsideHanoi(
                command.Latitude,
                command.Longitude))
            .WithName("Location")
            .WithMessage("Tọa độ phải nằm trong phạm vi Hà Nội.");
    }
}
