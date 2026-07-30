using FluentValidation;

namespace UrbanIssue.Application.Features.Dashboard.Common;

public abstract class DashboardDateRangeValidator<T>
    : AbstractValidator<T>
    where T : IDashboardDateRangeQuery
{
    private const int MaximumPeriodInDays = 366;

    protected DashboardDateRangeValidator()
    {
        RuleFor(query => query)
            .Must(HaveValidDateOrder)
            .WithMessage(
                "FromDate phải nhỏ hơn hoặc bằng ToDate.");

        RuleFor(query => query)
            .Must(HaveValidPeriodLength)
            .WithMessage(
                $"Khoảng thời gian thống kê không được vượt quá "
                + $"{MaximumPeriodInDays} ngày.");
    }

    private static bool HaveValidDateOrder(
        T query)
    {
        if (!query.FromDate.HasValue
            || !query.ToDate.HasValue)
        {
            return true;
        }

        return query.FromDate.Value
            <= query.ToDate.Value;
    }

    private static bool HaveValidPeriodLength(
        T query)
    {
        var range =
            DashboardDateRangeResolver.Resolve(
                query.FromDate,
                query.ToDate);

        var numberOfDays =
            range.ToDate.DayNumber
            - range.FromDate.DayNumber
            + 1;

        return numberOfDays
            is >= 1 and <= MaximumPeriodInDays;
    }
}
