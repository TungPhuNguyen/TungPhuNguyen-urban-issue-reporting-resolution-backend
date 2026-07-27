using FluentValidation;

namespace UrbanIssue.Application.Features.Users.Admin.GetAdminUsers;

public sealed class GetAdminUsersQueryValidator
    : AbstractValidator<GetAdminUsersQuery>
{
    private static readonly string[] AllowedRoles =
    [
        "Admin",
        "Staff",
        "Citizen"
    ];

    public GetAdminUsersQueryValidator()
    {
        RuleFor(x => x.Search)
            .MaximumLength(200)
            .WithMessage(
                "Từ khóa không được vượt quá 200 ký tự.")
            .When(x =>
                !string.IsNullOrWhiteSpace(x.Search));

        RuleFor(x => x.RoleName)
            .Must(roleName =>
                AllowedRoles.Contains(
                    roleName!,
                    StringComparer.OrdinalIgnoreCase))
            .WithMessage(
                "RoleName chỉ nhận Admin, Staff hoặc Citizen.")
            .When(x =>
                !string.IsNullOrWhiteSpace(x.RoleName));

        RuleFor(x => x.DepartmentId)
            .GreaterThan(0)
            .WithMessage(
                "ID phòng ban phải lớn hơn 0.")
            .When(x => x.DepartmentId.HasValue);

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(
                "Số trang phải lớn hơn hoặc bằng 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(
                "Số phần tử trên trang phải từ 1 đến 100.");
    }
}
