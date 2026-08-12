using FluentValidation;

namespace UrbanIssue.Application.Features.Departments.DeleteDepartment;

public sealed class DeleteDepartmentCommandValidator
    : AbstractValidator<DeleteDepartmentCommand>
{
    public DeleteDepartmentCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0)
            .WithMessage(
                "ID đơn vị xử lý phải lớn hơn 0.");
    }
}
