namespace UrbanIssue.API.Contracts.Areas;

public sealed record UpdateAreaRequest(
    string Name,
    string Code,
    int? ParentAreaId,
    bool IsActive);
