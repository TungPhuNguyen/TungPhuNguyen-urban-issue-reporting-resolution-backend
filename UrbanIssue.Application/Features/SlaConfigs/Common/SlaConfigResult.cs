using UrbanIssue.Domain.Enums;

namespace UrbanIssue.Application.Features.SlaConfigs.Common;

public sealed record SlaConfigResult(
    int Id,
    int CategoryId,
    string CategoryName,
    ReportPriority Priority,
    int DurationHours,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
