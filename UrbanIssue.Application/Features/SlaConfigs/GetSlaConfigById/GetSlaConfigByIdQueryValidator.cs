using FluentValidation;

namespace UrbanIssue.Application.Features.SlaConfigs.GetSlaConfigById;

public sealed class GetSlaConfigByIdQueryValidator
    : AbstractValidator<GetSlaConfigByIdQuery>
{
    public GetSlaConfigByIdQueryValidator()
    {
        RuleFor(query => query.Id)
            .GreaterThan(0)
            .WithMessage(
                "ID cấu hình SLA phải lớn hơn 0.");
    }
}
