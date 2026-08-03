using FluentValidation;

namespace UrbanIssue.Application.Features.Auth.ChangePassword;

public sealed class ChangePasswordCommandValidator
    : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(command => command.CurrentPassword).NotEmpty();
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
        RuleFor(command => command.NewPassword)
            .NotEqual(command => command.CurrentPassword)
            .WithMessage("Mật khẩu mới phải khác mật khẩu hiện tại.");
    }
}
