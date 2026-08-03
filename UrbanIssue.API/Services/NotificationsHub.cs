using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace UrbanIssue.API.Services;

[Authorize]
public sealed class NotificationsHub : Hub
{
}
