using FluentValidation;

namespace UrbanIssue.Application.Features.Users.Admin.ChangeUserStatus;

public sealed class ChangeUserStatusCommandValidator
    : AbstractValidator<ChangeUserStatusCommand>
{
    public ChangeUserStatusCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty)
            .WithMessage(
                "ID người dùng không hợp lệ.");

        RuleFor(x => x.Reason)
            .NotEmpty()
            .WithMessage(
                "Lý do thay đổi trạng thái không được để trống.")
            .MinimumLength(5)
            .WithMessage(
                "Lý do phải có ít nhất 5 ký tự.")
            .MaximumLength(500)
            .WithMessage(
                "Lý do không được vượt quá 500 ký tự.");
    }
}
