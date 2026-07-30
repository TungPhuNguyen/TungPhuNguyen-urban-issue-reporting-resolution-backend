using FluentValidation;

namespace UrbanIssue.Application.Features.Users.Admin.UpdateStaff;

public sealed class UpdateStaffCommandValidator
    : AbstractValidator<UpdateStaffCommand>
{
    public UpdateStaffCommandValidator()
    {
        RuleFor(x => x.StaffId)
            .NotEqual(Guid.Empty)
            .WithMessage(
                "ID Staff không hợp lệ.");

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

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0)
            .WithMessage(
                "ID phòng ban phải lớn hơn 0.");
    }
}
