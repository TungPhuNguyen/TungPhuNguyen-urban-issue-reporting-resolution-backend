using FluentValidation;

namespace UrbanIssue.Application.Features.Auth.VerifyEmail;

public sealed class VerifyEmailCommandValidator
    : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);
        RuleFor(command => command.Token).NotEmpty().MaximumLength(200);
    }
}
