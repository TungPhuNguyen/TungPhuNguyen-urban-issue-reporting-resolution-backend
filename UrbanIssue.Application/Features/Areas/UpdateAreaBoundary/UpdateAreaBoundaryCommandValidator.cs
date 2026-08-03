using FluentValidation;

namespace UrbanIssue.Application.Features.Areas.UpdateAreaBoundary;

public sealed class UpdateAreaBoundaryCommandValidator
    : AbstractValidator<UpdateAreaBoundaryCommand>
{
    public UpdateAreaBoundaryCommandValidator()
    {
        RuleFor(command => command.AreaId).GreaterThan(0);
        RuleFor(command => command.GeoJson)
            .MaximumLength(5_000_000)
            .When(command => command.GeoJson is not null)
            .WithMessage("GeoJSON không được vượt quá 5.000.000 ký tự.");
    }
}
