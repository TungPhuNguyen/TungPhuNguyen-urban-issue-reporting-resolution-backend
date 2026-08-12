using FluentValidation;

namespace UrbanIssue.Application.Features.Reports.Comments.GetReportComments;

public sealed class GetReportCommentsQueryValidator
    : AbstractValidator<GetReportCommentsQuery>
{
    public GetReportCommentsQueryValidator()
    {
        RuleFor(query => query.ReportId)
            .NotEqual(Guid.Empty)
            .WithMessage("ID báo cáo không hợp lệ.");

        RuleFor(query => query.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(
                "Số trang phải lớn hơn hoặc bằng 1.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage(
                "Số phần tử trên mỗi trang phải nằm trong khoảng từ 1 đến 100.");
    }
}
