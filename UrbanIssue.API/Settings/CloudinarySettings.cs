namespace UrbanIssue.API.Settings;

public sealed class CloudinarySettings
{
    public const string SectionName = "FileStorage:Cloudinary";

    public string CloudName { get; init; } = string.Empty;

    public string ApiKey { get; init; } = string.Empty;

    public string ApiSecret { get; init; } = string.Empty;
}
