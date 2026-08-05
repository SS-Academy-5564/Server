using Microsoft.AspNetCore.SignalR;
using Pulse.API.Hubs;
using Pulse.BL.Common.Notifications;
using Pulse.BL.Common.Security;
using Pulse.DAL.Commands.Monitors;
using Pulse.DAL.Common.Constants;

namespace Pulse.API.Common.Notifications;

public class SignalrNotificationService : INotificationService
{
    private readonly IHubContext<PulseNotificationHub, INotificationClient> _hubContext;


    public SignalrNotificationService(IHubContext<PulseNotificationHub, INotificationClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyAsync(Guid organizationId, UpdateMonitorAfterPollInput monitor, CancellationToken ct)
    {
        await _hubContext.Clients.Group(organizationId.ToString()).SendUpdatedMonitorAsync(monitor);
    }

    public async Task NotifyAsync(Guid organizationId,List<UpdateMonitorAfterPollInput> monitors, CancellationToken ct)
    {
        await _hubContext.Clients.Groups(organizationId.ToString()).SendUpdatedMonitorsAsync(monitors);
    }
}
