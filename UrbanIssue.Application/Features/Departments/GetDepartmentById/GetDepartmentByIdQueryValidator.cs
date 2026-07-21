using FluentValidation;

namespace UrbanIssue.Application.Features.Departments.GetDepartmentById;

public sealed class GetDepartmentByIdQueryValidator
    : AbstractValidator<GetDepartmentByIdQuery>
{
    public GetDepartmentByIdQueryValidator()
    {
        RuleFor(query => query.Id)
            .GreaterThan(0)
            .WithMessage(
                "ID đơn vị xử lý phải lớn hơn 0.");
    }
}
