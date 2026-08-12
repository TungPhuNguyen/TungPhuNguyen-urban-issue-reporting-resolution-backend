using UrbanIssue.Application.Common.Security;

namespace UrbanIssue.Application.Tests.Security;

public sealed class OneTimeTokenTests
{
    [Fact]
    public void Generate_CreatesDifferentUrlSafeTokens()
    {
        var first = OneTimeToken.Generate();
        var second = OneTimeToken.Generate();

        Assert.NotEqual(first, second);
        Assert.DoesNotContain("+", first);
        Assert.DoesNotContain("/", first);
        Assert.DoesNotContain("=", first);
    }

    [Fact]
    public void Matches_AcceptsOriginalToken_AndRejectsAnotherToken()
    {
        var token = OneTimeToken.Generate();
        var hash = OneTimeToken.Hash(token);

        Assert.True(OneTimeToken.Matches(token, hash));
        Assert.False(OneTimeToken.Matches(OneTimeToken.Generate(), hash));
    }
}
