using System.Security.Cryptography;
using System.Text;

namespace UrbanIssue.Application.Common.Security;

public static class OneTimeToken
{
    public static string Generate()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static string Hash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);
        return Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }

    public static bool Matches(string token, string expectedHash)
    {
        if (string.IsNullOrWhiteSpace(token)
            || string.IsNullOrWhiteSpace(expectedHash))
        {
            return false;
        }

        var actualHash = Encoding.ASCII.GetBytes(Hash(token));
        var storedHash = Encoding.ASCII.GetBytes(expectedHash);
        return actualHash.Length == storedHash.Length
            && CryptographicOperations.FixedTimeEquals(actualHash, storedHash);
    }
}
