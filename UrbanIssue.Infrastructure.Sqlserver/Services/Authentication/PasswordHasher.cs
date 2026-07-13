using Microsoft.AspNetCore.Identity;

using ApplicationPasswordHasher =
    UrbanIssue.Application.Common.Interfaces.Authentication.IPasswordHasher;

using IdentityPasswordHasher =
    Microsoft.AspNetCore.Identity.PasswordHasher<object>;

namespace UrbanIssue.Infrastructure.Sqlserver.Services.Authentication;

public sealed class PasswordHasher : ApplicationPasswordHasher
{
    private static readonly object UserContext = new();

    private readonly IdentityPasswordHasher _passwordHasher = new();

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        return _passwordHasher.HashPassword(
            UserContext,
            password);
    }

    public bool Verify(
        string password,
        string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password)
            || string.IsNullOrWhiteSpace(passwordHash))
        {
            return false;
        }

        var result = _passwordHasher.VerifyHashedPassword(
            UserContext,
            passwordHash,
            password);

        return result is
            PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
