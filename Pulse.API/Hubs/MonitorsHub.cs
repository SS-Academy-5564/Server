using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Pulse.API.Common.Security;

using Pulse.DAL.Common.Constants;

namespace Pulse.API.Hubs;

[Authorize]
public class BaseNotificationHub(CurrentUserService userService) : Hub
{
    public override async Task OnConnectedAsync()
    {
        Guid organizationId = userService.OrganizationId ?? SeededIds.Organizations.Default;
        await Groups.AddToGroupAsync(Context.ConnectionId, organizationId.ToString());
        await base.OnConnectedAsync();
    }

    public async Task SendMessage(List<MonitorUpdateResult> monitors)
    {
        Guid organizationId = userService.OrganizationId ?? SeededIds.Organizations.Default;
        await Clients.Groups(organizationId.ToString()).SendAsync("ReceiveMessage", monitors);
    }
}
