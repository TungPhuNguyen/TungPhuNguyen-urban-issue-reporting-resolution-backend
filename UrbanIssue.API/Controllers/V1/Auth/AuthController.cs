using MediatR;
using Microsoft.AspNetCore.Mvc;
using UrbanIssue.Application.Features.Auth.Login;
using UrbanIssue.Application.Features.Auth.Register;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using UrbanIssue.Application
    .Features
    .Auth
    .GetCurrentUser;
using UrbanIssue.Application.Features.Auth.Logout;
using UrbanIssue.Application.Features.Auth.RefreshAccessToken;



namespace UrbanIssue.API.Controllers.V1.Auth;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController
    : ControllerBase
{
    private readonly ISender _sender;

    public AuthController(
        ISender sender)
    {
        _sender =
            sender;
    }

    /// <summary>
    /// Đăng ký tài khoản công dân mới.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(
        typeof(RegisterResult),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RegisterResult>>
        Register(
            [FromBody] RegisterCommand command,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                command,
                cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            result);
    }

    /// <summary>
    /// Đăng nhập vào hệ thống.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(
        typeof(LoginResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoginResult>>
        Login(
            [FromBody] LoginCommand command,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                command,
                cancellationToken);

        return Ok(
            result);
    }
    /// <summary>
    /// Lấy thông tin tài khoản đang đăng nhập.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(
        typeof(GetCurrentUserResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<
        ActionResult<GetCurrentUserResult>>
        GetCurrentUser(
            CancellationToken cancellationToken)
    {
        var userIdClaim =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(
                userIdClaim,
                out var userId))
        {
            throw new UnauthorizedAccessException(
                "Access token không chứa UserId hợp lệ.");
        }

        var result =
            await _sender.Send(
                new GetCurrentUserQuery(
                    userId),
                cancellationToken);

        return Ok(
            result);
    }
    /// <summary>
    /// Tạo access token và refresh token mới.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("refresh-token")]
    [ProducesResponseType(
        typeof(RefreshAccessTokenResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status500InternalServerError)]
    public async Task<
        ActionResult<RefreshAccessTokenResult>>
        RefreshAccessToken(
            [FromBody]
        RefreshAccessTokenCommand command,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                command,
                cancellationToken);

        return Ok(
            result);
    }
    /// <summary>
    /// Đăng xuất và thu hồi refresh token.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("logout")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult>
        Logout(
            [FromBody] LogoutCommand command,
            CancellationToken cancellationToken)
    {
        await _sender.Send(
            command,
            cancellationToken);

        return NoContent();
    }
}
