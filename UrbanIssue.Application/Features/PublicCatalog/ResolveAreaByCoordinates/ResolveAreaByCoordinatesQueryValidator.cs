using FluentValidation;
using UrbanIssue.Application.Common.Rules;

namespace UrbanIssue.Application.Features.PublicCatalog.ResolveAreaByCoordinates;

public sealed class ResolveAreaByCoordinatesQueryValidator
    : AbstractValidator<ResolveAreaByCoordinatesQuery>
{
    public ResolveAreaByCoordinatesQueryValidator()
    {
        RuleFor(query => query)
            .Must(query => HanoiLocationRules.IsInsideHanoi(
                query.Latitude,
                query.Longitude))
            .WithName("Location")
            .WithMessage("Tọa độ phải nằm trong phạm vi Hà Nội.");
    }
}
