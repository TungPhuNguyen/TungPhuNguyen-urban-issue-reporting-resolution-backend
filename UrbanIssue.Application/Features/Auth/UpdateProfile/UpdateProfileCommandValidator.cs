using FluentValidation;

namespace UrbanIssue.Application.Features.Auth.UpdateProfile;

public sealed class UpdateProfileCommandValidator
    : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(command => command.FullName)
            .NotEmpty().MaximumLength(150);
        RuleFor(command => command.PhoneNumber)
            .Matches("^[0-9+ .()-]{8,20}$")
            .WithMessage("Số điện thoại không hợp lệ.")
            .When(command => !string.IsNullOrWhiteSpace(command.PhoneNumber));
    }
}
