using FluentValidation;

namespace UrbanIssue.Application.Features.RoutingRules.UpdateRoutingRule;

public sealed class UpdateRoutingRuleCommandValidator
    : AbstractValidator<UpdateRoutingRuleCommand>
{
    public UpdateRoutingRuleCommandValidator()
    {
        RuleFor(command => command.Id)
            .GreaterThan(0)
            .WithMessage(
                "ID quy tắc định tuyến phải lớn hơn 0.");

        RuleFor(command => command.CategoryId)
            .GreaterThan(0)
            .WithMessage(
                "ID loại sự cố phải lớn hơn 0.");

        RuleFor(command => command.AreaId)
            .GreaterThan(0)
            .WithMessage(
                "ID khu vực phải lớn hơn 0.");

        RuleFor(command => command.DepartmentId)
            .GreaterThan(0)
            .WithMessage(
                "ID đơn vị xử lý phải lớn hơn 0.");

        RuleFor(command => command.PriorityOrder)
            .GreaterThan(0)
            .WithMessage(
                "Thứ tự ưu tiên phải lớn hơn 0.");
    }
}
