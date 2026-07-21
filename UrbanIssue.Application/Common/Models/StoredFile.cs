namespace UrbanIssue.Application.Common.Models;

public sealed record StoredFile(
    string StorageKey,
    string PublicUrl);
