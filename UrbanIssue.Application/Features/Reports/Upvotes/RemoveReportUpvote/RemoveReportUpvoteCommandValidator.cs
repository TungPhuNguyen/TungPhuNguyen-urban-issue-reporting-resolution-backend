using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.Upvotes.RemoveReportUpvote;

public sealed class RemoveReportUpvoteCommandValidator
    : AbstractValidator<RemoveReportUpvoteCommand>
{
    public RemoveReportUpvoteCommandValidator()
    {
        RuleFor(command => command.ReportId)
            .NotEqual(Guid.Empty)
            .WithMessage("ID báo cáo không hợp lệ.");
    }
}
