using System.Security.Claims;
using UrbanIssue.Application.Common.Interfaces.Authentication;

namespace UrbanIssue.API.Services;

public sealed class CurrentUserService
    : ICurrentUserService
{
    private readonly IHttpContextAccessor
        _httpContextAccessor;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor =
            httpContextAccessor;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor
            .HttpContext?
            .User
            .Identity?
            .IsAuthenticated
        == true;

    public string? Role =>
        _httpContextAccessor
            .HttpContext?
            .User
            .FindFirstValue(
                ClaimTypes.Role);

    public Guid UserId
    {
        get
        {
            var userIdValue =
                _httpContextAccessor
                    .HttpContext?
                    .User
                    .FindFirstValue(
                        ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(
                    userIdValue,
                    out var userId))
            {
                throw new UnauthorizedAccessException(
                    "Access token không chứa UserId hợp lệ.");
            }

            return userId;
        }
    }
}
