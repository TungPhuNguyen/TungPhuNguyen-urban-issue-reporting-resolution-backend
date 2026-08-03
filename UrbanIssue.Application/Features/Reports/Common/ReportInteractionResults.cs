using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.Reports.Common;

public sealed record ReportAllowedActionsResult(
    bool CanEdit,
    bool CanCancel,
    bool CanAssign,
    bool CanReassign,
    bool CanClassify,
    bool CanAccept,
    bool CanStartProcessing,
    bool CanAddProgress,
    bool CanResolve,
    bool CanClose,
    bool CanComplain,
    bool CanReviewComplaint,
    bool CanUpvote);

public sealed record ReportResolutionResult(
    string? Note,
    Guid? ResolvedByUserId,
    string? ResolvedByUserName,
    DateTime ResolvedAt,
    IReadOnlyList<string> ImageUrls);

public sealed record ComplaintResult(
    int Id,
    ComplaintStatus Status,
    string Reason,
    string? AdminDecisionReason,
    Guid? ResolvedByAdminId,
    string? ResolvedByAdminName,
    DateTime CreatedAt,
    DateTime? ResolvedAt,
    IReadOnlyList<string> ImageUrls);
