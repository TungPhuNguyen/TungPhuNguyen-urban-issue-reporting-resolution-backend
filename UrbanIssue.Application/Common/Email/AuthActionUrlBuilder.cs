namespace UrbanIssue.Application.Common.Email;

public static class AuthActionUrlBuilder
{
    public static string Build(
        string frontendBaseUrl,
        string path,
        string email,
        string token)
    {
        return $"{frontendBaseUrl.TrimEnd('/')}/{path.TrimStart('/')}"
            + $"?email={Uri.EscapeDataString(email)}"
            + $"&token={Uri.EscapeDataString(token)}";
    }
}
