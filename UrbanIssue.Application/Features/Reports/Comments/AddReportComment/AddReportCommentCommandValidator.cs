using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.Comments.AddReportComment;

public sealed class AddReportCommentCommandValidator
    : AbstractValidator<AddReportCommentCommand>
{
    public AddReportCommentCommandValidator()
    {
        RuleFor(command => command.ReportId)
            .NotEqual(Guid.Empty)
            .WithMessage("ID báo cáo không hợp lệ.");

        RuleFor(command => command.Content)
            .NotEmpty()
            .WithMessage("Nội dung bình luận không được để trống.")
            .MaximumLength(1000)
            .WithMessage(
                "Nội dung bình luận không được vượt quá 1000 ký tự.");
    }
}
