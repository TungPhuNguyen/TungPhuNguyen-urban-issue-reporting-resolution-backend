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
using UrbanIssue.Application.Features.Auth.ChangePassword;
using UrbanIssue.Application.Features.Auth.UpdateProfile;
using UrbanIssue.Application.Features.Auth.ForgotPassword;
using UrbanIssue.Application.Features.Auth.ResetPassword;
using UrbanIssue.Application.Features.Auth.ResendVerificationEmail;
using UrbanIssue.Application.Features.Auth.VerifyEmail;
using UrbanIssue.API.Contracts.Auth;



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

    [Authorize]
    [HttpPut("profile")]
    public async Task<ActionResult<GetCurrentUserResult>> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(
            new UpdateProfileCommand(request.FullName, request.PhoneNumber),
            cancellationToken);
        return Ok(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(new ChangePasswordCommand(
            request.CurrentPassword,
            request.NewPassword,
            request.ConfirmNewPassword), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Gửi liên kết xác minh email mới mà không làm lộ email có tồn tại hay không.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("resend-verification-email")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ResendVerificationEmail(
        [FromBody] EmailRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new ResendVerificationEmailCommand(request.Email),
            cancellationToken);

        return Accepted();
    }

    /// <summary>
    /// Xác minh địa chỉ email bằng token dùng một lần.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("verify-email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> VerifyEmail(
        [FromBody] EmailVerificationRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new VerifyEmailCommand(request.Email, request.Token),
            cancellationToken);

        return NoContent();
    }

    /// <summary>
    /// Gửi liên kết đặt lại mật khẩu mà không làm lộ email có tồn tại hay không.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] EmailRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new ForgotPasswordCommand(request.Email),
            cancellationToken);

        return Accepted();
    }

    /// <summary>
    /// Đặt mật khẩu mới bằng token dùng một lần và thu hồi refresh token cũ.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await _sender.Send(
            new ResetPasswordCommand(
                request.Email,
                request.Token,
                request.NewPassword,
                request.ConfirmNewPassword),
            cancellationToken);

        return NoContent();
    }
}
