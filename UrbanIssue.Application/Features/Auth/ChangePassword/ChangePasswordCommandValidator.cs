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
            .WithMessage("Mật khẩu mới không được để trống.")

            .MinimumLength(8)
            .WithMessage("Mật khẩu mới phải có ít nhất 8 ký tự.")

            .MaximumLength(100)
            .WithMessage("Mật khẩu mới không được vượt quá 100 ký tự.")

            .Matches("[A-Z]")
            .WithMessage("Mật khẩu mới phải chứa ít nhất một chữ hoa.")

            .Matches("[a-z]")
            .WithMessage("Mật khẩu mới phải chứa ít nhất một chữ thường.")

            .Matches("[0-9]")
            .WithMessage("Mật khẩu mới phải chứa ít nhất một chữ số.")

            .Matches("[^a-zA-Z0-9]")
            .WithMessage("Mật khẩu mới phải chứa ít nhất một ký tự đặc biệt.");
        RuleFor(command => command.ConfirmNewPassword)
            .Equal(command => command.NewPassword)
            .WithMessage("Xác nhận mật khẩu mới không khớp.");
        RuleFor(command => command.NewPassword)
            .NotEqual(command => command.CurrentPassword)
            .WithMessage("Mật khẩu mới phải khác mật khẩu hiện tại.");
    }
}
