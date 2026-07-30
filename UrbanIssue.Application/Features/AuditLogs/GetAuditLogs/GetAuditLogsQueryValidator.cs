using FluentValidation;

namespace UrbanIssue.Application.Features.AuditLogs.GetAuditLogs;

public sealed class GetAuditLogsQueryValidator
    : AbstractValidator<GetAuditLogsQuery>
{
    public GetAuditLogsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEqual(Guid.Empty)
            .WithMessage("ID người dùng không hợp lệ.")
            .When(x => x.UserId.HasValue);

        RuleFor(x => x.Action)
            .MaximumLength(100)
            .WithMessage(
                "Action không được vượt quá 100 ký tự.")
            .When(x =>
                !string.IsNullOrWhiteSpace(x.Action));

        RuleFor(x => x.EntityType)
            .MaximumLength(50)
            .WithMessage(
                "EntityType không được vượt quá 50 ký tự.")
            .When(x =>
                !string.IsNullOrWhiteSpace(x.EntityType));

        RuleFor(x => x.EntityId)
            .MaximumLength(50)
            .WithMessage(
                "EntityId không được vượt quá 50 ký tự.")
            .When(x =>
                !string.IsNullOrWhiteSpace(x.EntityId));

        RuleFor(x => x.CreatedFrom)
            .LessThanOrEqualTo(x => x.CreatedTo)
            .WithMessage(
                "Thời gian bắt đầu phải nhỏ hơn hoặc bằng thời gian kết thúc.")
            .When(x =>
                x.CreatedFrom.HasValue
                && x.CreatedTo.HasValue);

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
