using FluentValidation;

namespace UrbanIssue.Application.Features.Auth.ResetPassword;

public sealed class ResetPasswordCommandValidator
    : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(command => command.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);
        RuleFor(command => command.Token).NotEmpty().MaximumLength(200);
        RuleFor(command => command.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(100)
            .Matches("[A-Z]")
            .Matches("[a-z]")
            .Matches("[0-9]")
            .Matches("[^a-zA-Z0-9]");
        RuleFor(command => command.ConfirmNewPassword)
            .Equal(command => command.NewPassword)
            .WithMessage("Xác nhận mật khẩu mới không khớp.");
    }
}
