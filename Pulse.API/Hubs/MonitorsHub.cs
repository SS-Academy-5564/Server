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
    public override async Task OnConnectedAsync()
    {
        Guid organizationId = _userService.OrganizationId ?? SeededIds.Organizations.Default;
        await Groups.AddToGroupAsync(Context.ConnectionId, organizationId.ToString());
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Guid organizationId = _userService.OrganizationId ?? SeededIds.Organizations.Default;
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, organizationId.ToString());
        await base.OnDisconnectedAsync(exception);
    }
}
