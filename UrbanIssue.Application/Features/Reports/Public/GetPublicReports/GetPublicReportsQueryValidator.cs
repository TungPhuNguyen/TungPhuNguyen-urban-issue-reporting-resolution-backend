using FluentValidation;
using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Public.GetPublicReports;

public sealed class GetPublicReportsQueryValidator
    : AbstractValidator<GetPublicReportsQuery>
{
    public GetPublicReportsQueryValidator()
    {
        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("ID loại sự cố phải lớn hơn 0.")
            .When(x => x.CategoryId.HasValue);

        RuleFor(x => x.AreaId)
            .GreaterThan(0)
            .WithMessage("ID khu vực phải lớn hơn 0.")
            .When(x => x.AreaId.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Trạng thái báo cáo không hợp lệ.")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.Status)
            .Must(status =>
                !status.HasValue
                || status.Value != ReportStatus.Rejected)
            .WithMessage(
                "Không thể truy vấn báo cáo Rejected trên API công khai.");

        RuleFor(x => x.CreatedFrom)
            .LessThanOrEqualTo(x => x.CreatedTo)
            .WithMessage(
                "Thời gian bắt đầu phải nhỏ hơn hoặc bằng thời gian kết thúc.")
            .When(x =>
                x.CreatedFrom.HasValue
                && x.CreatedTo.HasValue);

        RuleFor(x => x.MinLatitude)
            .InclusiveBetween(-90m, 90m)
            .WithMessage(
                "Vĩ độ nhỏ nhất phải nằm trong khoảng từ -90 đến 90.")
            .When(x => x.MinLatitude.HasValue);

        RuleFor(x => x.MaxLatitude)
            .InclusiveBetween(-90m, 90m)
            .WithMessage(
                "Vĩ độ lớn nhất phải nằm trong khoảng từ -90 đến 90.")
            .When(x => x.MaxLatitude.HasValue);

        RuleFor(x => x.MinLongitude)
            .InclusiveBetween(-180m, 180m)
            .WithMessage(
                "Kinh độ nhỏ nhất phải nằm trong khoảng từ -180 đến 180.")
            .When(x => x.MinLongitude.HasValue);

        RuleFor(x => x.MaxLongitude)
            .InclusiveBetween(-180m, 180m)
            .WithMessage(
                "Kinh độ lớn nhất phải nằm trong khoảng từ -180 đến 180.")
            .When(x => x.MaxLongitude.HasValue);

        RuleFor(x => x)
            .Must(HaveAllMapBoundsOrNone)
            .WithMessage(
                "Khi lọc theo vùng bản đồ phải cung cấp đủ "
                + "MinLatitude, MaxLatitude, MinLongitude và MaxLongitude.");

        RuleFor(x => x)
            .Must(HaveValidMapBounds)
            .WithMessage(
                "MinLatitude phải nhỏ hơn MaxLatitude và "
                + "MinLongitude phải nhỏ hơn MaxLongitude.");

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage(
                "Số trang phải lớn hơn hoặc bằng 1.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 500)
            .WithMessage(
                "Số phần tử trên trang phải nằm trong khoảng từ 1 đến 500.");
    }

    private static bool HaveAllMapBoundsOrNone(
        GetPublicReportsQuery query)
    {
        var providedCount = new bool[]
        {
            query.MinLatitude.HasValue,
            query.MaxLatitude.HasValue,
            query.MinLongitude.HasValue,
            query.MaxLongitude.HasValue
        }
        .Count(value => value);

        return providedCount is 0 or 4;
    }

    private static bool HaveValidMapBounds(
        GetPublicReportsQuery query)
    {
        if (!query.MinLatitude.HasValue
            && !query.MaxLatitude.HasValue
            && !query.MinLongitude.HasValue
            && !query.MaxLongitude.HasValue)
        {
            return true;
        }

        if (!HaveAllMapBoundsOrNone(query))
        {
            return true;
        }

        return query.MinLatitude!.Value
                   < query.MaxLatitude!.Value
               && query.MinLongitude!.Value
                   < query.MaxLongitude!.Value;
    }
}
