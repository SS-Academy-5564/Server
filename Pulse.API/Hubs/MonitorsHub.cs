using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Pulse.API.Common.Security;
using Pulse.BL.Features.Monitors;
using Pulse.DAL.Common.Constants;

namespace Pulse.API.Hubs;

[Authorize]
public sealed class PulseNotificationHub : Hub<INotificationHub>
{
    private readonly CurrentUserService _userService;
    public PulseNotificationHub(CurrentUserService userService)
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


    public async Task<MonitorListResult> SendUpdatedMonitorAsync(MonitorListResult monitor){
        Guid organizationId = _userService.OrganizationId ?? SeededIds.Organizations.Default;
        await Clients.Groups(organizationId.ToString()).SendUpdatedMonitorAsync(monitor);

        return monitor;
    }

    /// <summary>
    /// Will be called by the Poller Worker
    /// </summary>
    /// <param name="monitors"></param>
    public async Task<List<MonitorListResult>> SendUpdatedMonitorsAsync(List<MonitorListResult> monitors){
        Guid organizationId = _userService.OrganizationId ?? SeededIds.Organizations.Default;
        await Clients.Groups(organizationId.ToString()).SendUpdatedMonitorsAsync(monitors);

        return monitors;
    }
}

public interface INotificationHub
{
    /// <summary>
    /// Will be called by the Poller Worker
    /// </summary>
    /// <param name="monitors"></param>
    Task SendUpdatedMonitorsAsync(List<MonitorListResult> monitors);
    Task SendUpdatedMonitorAsync(MonitorListResult monitor);
}
