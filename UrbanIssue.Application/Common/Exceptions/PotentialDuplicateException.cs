using UrbanIssue.Application.Features.Reports.CheckDuplicateReports;

namespace UrbanIssue.Application.Common.Exceptions;

public sealed class PotentialDuplicateException : Exception
{
    public PotentialDuplicateException(
        CheckDuplicateReportsResult duplicates)
        : base(
            "Phát hiện phản ánh cùng loại trong bán kính 100 mét. "
            + "Hãy xem/upvote phản ánh hiện có hoặc gửi lại với "
            + "ConfirmPossibleDuplicate = true.")
    {
        Duplicates = duplicates;
    }

    public CheckDuplicateReportsResult Duplicates { get; }
}
