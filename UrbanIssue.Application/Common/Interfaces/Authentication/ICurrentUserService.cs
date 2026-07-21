namespace UrbanIssue.Application.Common.Interfaces.Authentication;

public interface ICurrentUserService
{
    Guid UserId { get; }

    bool IsAuthenticated { get; }

    string? Role { get; }
}
