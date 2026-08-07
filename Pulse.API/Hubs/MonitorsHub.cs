using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Pulse.BL.Common.Security;
using Pulse.DAL.Common.Constants;

namespace Pulse.API.Hubs;

[Authorize]
public sealed class PulseNotificationHub : Hub<INotificationClient>
{
    private readonly ICurrentUserService _userService;

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
        Guid organizationId = _userService.OrganizationId ?? SeededIds.Organizations.Default;
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
        Guid organizationId = _userService.OrganizationId ?? SeededIds.Organizations.Default;
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, organizationId.ToString());
        await base.OnDisconnectedAsync(exception);
    }
}
