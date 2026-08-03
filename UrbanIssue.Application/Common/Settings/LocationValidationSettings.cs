namespace UrbanIssue.Application.Common.Settings;

public sealed class LocationValidationSettings
{
    public const string SectionName = "LocationValidation";

    public bool RequireWardBoundary { get; init; }
}
