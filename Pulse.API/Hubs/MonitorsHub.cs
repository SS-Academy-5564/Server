using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Pulse.BL.Common.Security;

namespace Pulse.API.Hubs;

/// <summary>
/// SignalR hub for dispatching monitor updates to authenticated clients.
/// </summary>
[Authorize]
public sealed class PulseNotificationHub : Hub<INotificationClient>
{
    private readonly ICurrentUserService _userService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PulseNotificationHub"/> class.
    /// </summary>
    /// <param name="userService">The user service used to resolve current organization context.</param>
    public PulseNotificationHub(ICurrentUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Adds the connected client to its organization notification group.
    /// </summary>
    /// <returns>A task that represents the asynchronous connection operation.</returns>
    public override async Task OnConnectedAsync()
    {
        if (_userService.OrganizationId is not { } organizationId || organizationId == Guid.Empty)
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, organizationId.ToString());
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// Removes the disconnected client from its organization notification group.
    /// </summary>
    /// <param name="exception">The exception that caused the disconnect, or <see langword="null"/> for a graceful disconnect.</param>
    /// <returns>A task that represents the asynchronous disconnection operation.</returns>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_userService.OrganizationId is { } organizationId && organizationId != Guid.Empty)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, organizationId.ToString());
        }

        await base.OnDisconnectedAsync(exception);
    }
}
