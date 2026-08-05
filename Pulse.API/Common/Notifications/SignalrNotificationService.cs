using Microsoft.AspNetCore.SignalR;
using Pulse.API.Hubs;
using Pulse.BL.Common.Notifications;
using Pulse.BL.Features.Polling;

namespace Pulse.API.Common.Notifications;

public class SignalrNotificationService : INotificationService
{
    private readonly IHubContext<PulseNotificationHub, INotificationClient> _hubContext;

    public SignalrNotificationService(IHubContext<PulseNotificationHub, INotificationClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyAsync(MonitorPollResult monitor, CancellationToken ct)
    {
        await _hubContext.Clients.Group(monitor.OrganizationId.ToString()).SendUpdatedMonitorAsync(monitor);
    }

    public async Task NotifyAsync(List<MonitorPollResult> monitors, CancellationToken ct)
    {
        var orgMonitorsGroups = monitors.GroupBy(monitor => monitor.OrganizationId);
        foreach (var group in orgMonitorsGroups)
        {
            await _hubContext.Clients.Group(group.Key.ToString()).SendUpdatedMonitorsAsync(group.ToList());
        }
    }
}
