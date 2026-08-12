using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.Upvotes.AddReportUpvote;

public sealed class AddReportUpvoteCommandValidator
    : AbstractValidator<AddReportUpvoteCommand>
{
    public AddReportUpvoteCommandValidator()
    {
        RuleFor(command => command.ReportId)
            .NotEqual(Guid.Empty)
            .WithMessage("ID báo cáo không hợp lệ.");
    }
}
