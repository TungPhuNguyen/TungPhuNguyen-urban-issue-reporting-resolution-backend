using FluentValidation;

namespace UrbanIssue.Application.Features.Users.Admin.CreateStaff;

public sealed class CreateStaffCommandValidator
    : AbstractValidator<CreateStaffCommand>
{
    public CreateStaffCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage(
                "Họ tên không được để trống.")
            .MaximumLength(150)
            .WithMessage(
                "Họ tên không được vượt quá 150 ký tự.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(
                "Email không được để trống.")
            .EmailAddress()
            .WithMessage(
                "Email không đúng định dạng.")
            .MaximumLength(256)
            .WithMessage(
                "Email không được vượt quá 256 ký tự.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(
                "Mật khẩu không được để trống.")
            .MinimumLength(8)
            .WithMessage(
                "Mật khẩu phải có ít nhất 8 ký tự.")
            .MaximumLength(100)
            .WithMessage(
                "Mật khẩu không được vượt quá 100 ký tự.")
            .Matches("[A-Z]")
            .WithMessage(
                "Mật khẩu phải có ít nhất một chữ hoa.")
            .Matches("[a-z]")
            .WithMessage(
                "Mật khẩu phải có ít nhất một chữ thường.")
            .Matches("[0-9]")
            .WithMessage(
                "Mật khẩu phải có ít nhất một chữ số.")
            .Matches("[^a-zA-Z0-9]")
            .WithMessage(
                "Mật khẩu phải có ít nhất một ký tự đặc biệt.");

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0)
            .WithMessage(
                "ID phòng ban phải lớn hơn 0.");
    }
}
