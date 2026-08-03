namespace UrbanIssue.Application.Common.Rules;

public static class HanoiLocationRules
{
    // Bounding box covers the administrative extent of Ha Noi.
    public const decimal MinimumLatitude = 20.45m;
    public const decimal MaximumLatitude = 21.65m;
    public const decimal MinimumLongitude = 104.75m;
    public const decimal MaximumLongitude = 106.05m;

    public static bool IsInsideHanoi(
        decimal latitude,
        decimal longitude)
    {
        return latitude is >= MinimumLatitude and <= MaximumLatitude
            && longitude is >= MinimumLongitude and <= MaximumLongitude;
    }
}
