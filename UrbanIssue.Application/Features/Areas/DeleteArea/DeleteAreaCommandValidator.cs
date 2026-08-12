using FluentValidation;

namespace UrbanIssue.Application.Features.Areas.DeleteArea;

public sealed class DeleteAreaCommandValidator
    : AbstractValidator<DeleteAreaCommand>
{
    public DeleteAreaCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0)
            .WithMessage(
                "ID khu vực phải lớn hơn 0.");
    }
}
