using FluentValidation;

namespace UrbanIssue.Application.Features.Departments.UpdateDepartment;

public sealed class UpdateDepartmentCommandValidator
    : AbstractValidator<UpdateDepartmentCommand>
{
    public UpdateDepartmentCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0)
            .WithMessage(
                "ID đơn vị xử lý phải lớn hơn 0.");

        RuleFor(command => command.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage(
                "Tên đơn vị xử lý không được để trống.")
            .MaximumLength(150)
            .WithMessage(
                "Tên đơn vị xử lý không được vượt quá 150 ký tự.");

        RuleFor(command => command.Description)
            .MaximumLength(1000)
            .WithMessage(
                "Mô tả không được vượt quá 1000 ký tự.")
            .When(command =>
                !string.IsNullOrWhiteSpace(
                    command.Description));
    }
}
