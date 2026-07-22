using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.Comments.DeleteReportComment;

public sealed class DeleteReportCommentCommandValidator
    : AbstractValidator<DeleteReportCommentCommand>
{
    public DeleteReportCommentCommandValidator()
    {
        RuleFor(command => command.ReportId)
            .NotEqual(Guid.Empty)
            .WithMessage("ID báo cáo không hợp lệ.");

        RuleFor(command => command.CommentId)
            .GreaterThan(0)
            .WithMessage("ID bình luận không hợp lệ.");
    }
}
