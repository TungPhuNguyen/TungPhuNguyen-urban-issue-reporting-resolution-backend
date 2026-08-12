using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.GetReportTimeline;

public sealed class GetReportTimelineQueryValidator
    : AbstractValidator<GetReportTimelineQuery>
{
    public GetReportTimelineQueryValidator()
    {
        RuleFor(query => query.ReportId)
            .NotEqual(Guid.Empty)
            .WithMessage(
                "ID báo cáo không hợp lệ.");
    }
}
