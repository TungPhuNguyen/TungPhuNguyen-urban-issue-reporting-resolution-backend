using FluentValidation;

namespace UrbanIssue.Application.Features.Users.Admin.GetAdminUserById;

public sealed class GetAdminUserByIdQueryValidator
    : AbstractValidator<GetAdminUserByIdQuery>
{
    public GetAdminUserByIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty)
            .WithMessage(
                "ID người dùng không hợp lệ.");
    }
}
