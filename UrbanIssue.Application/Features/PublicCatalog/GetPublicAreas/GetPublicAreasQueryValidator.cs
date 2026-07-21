using FluentValidation;

namespace UrbanIssue.Application.Features.PublicCatalog.GetPublicAreas;

public sealed class GetPublicAreasQueryValidator
    : AbstractValidator<GetPublicAreasQuery>
{
    public GetPublicAreasQueryValidator()
    {
        RuleFor(query =>
                query.ParentAreaId)
            .GreaterThan(0)
            .WithMessage(
                "ID khu vực cha phải lớn hơn 0.")
            .When(query =>
                query.ParentAreaId.HasValue);
    }
}
