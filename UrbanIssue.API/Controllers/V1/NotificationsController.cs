using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrbanIssue.Application.Common.Models;
using UrbanIssue.Application.Features.Notifications.Common;
using UrbanIssue.Application.Features.Notifications.GetMyNotifications;
using UrbanIssue.Application.Features.Notifications.MarkAllNotificationsAsRead;
using UrbanIssue.Application.Features.Notifications.MarkNotificationAsRead;

namespace UrbanIssue.API.Controllers.V1;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public sealed class NotificationsController
    : ControllerBase
{
    private readonly ISender _sender;

    public NotificationsController(
        ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(PagedResult<NotificationResult>),
        StatusCodes.Status200OK)]
    public async Task<
        ActionResult<PagedResult<NotificationResult>>>
        GetMyNotifications(
            [FromQuery]
            GetMyNotificationsQuery query,

            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                query,
                cancellationToken);

        return Ok(result);
    }

    [HttpPatch("{id:int}/read")]
    [ProducesResponseType(
        typeof(NotificationResult),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotificationResult>>
        MarkAsRead(
            int id,
            CancellationToken cancellationToken)
    {
        var result =
            await _sender.Send(
                new MarkNotificationAsReadCommand(id),
                cancellationToken);

        return Ok(result);
    }

    [HttpPatch("read-all")]
    [ProducesResponseType(
        typeof(object),
        StatusCodes.Status200OK)]
    public async Task<IActionResult>
        MarkAllAsRead(
            CancellationToken cancellationToken)
    {
        var updatedCount =
            await _sender.Send(
                new MarkAllNotificationsAsReadCommand(),
                cancellationToken);

        return Ok(new
        {
            updatedCount
        });
    }
}
