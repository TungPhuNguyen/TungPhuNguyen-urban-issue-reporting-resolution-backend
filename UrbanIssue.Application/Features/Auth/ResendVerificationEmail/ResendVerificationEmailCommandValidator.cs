using FluentValidation;

namespace UrbanIssue.Application.Features.Auth.ResendVerificationEmail;

public sealed class ResendVerificationEmailCommandValidator
    : AbstractValidator<ResendVerificationEmailCommand>
{
    public ResendVerificationEmailCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);
    }
}
