using FluentValidation;

namespace UrbanIssue.Application.Features.Areas.GetAreaById;

public sealed class GetAreaByIdQueryValidator
    : AbstractValidator<GetAreaByIdQuery>
{
    public GetAreaByIdQueryValidator()
    {
        RuleFor(query => query.Id)
            .GreaterThan(0)
            .WithMessage(
                "ID khu vực phải lớn hơn 0.");
    }
}
