using Microsoft.AspNetCore.SignalR;
using Pulse.API.Hubs;
using Pulse.BL.Common.Notifications;
using Pulse.BL.Features.Polling;

namespace Pulse.API.Common.Notifications;

public class SignalrNotificationService : IMonitorNotificationService, IBatchMonitorNotificationService
{
    private readonly IHubContext<PulseNotificationHub, INotificationClient> _hubContext;

    public SignalrNotificationService(IHubContext<PulseNotificationHub, INotificationClient> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <summary>
    /// Sends an updated monitor notification to the specified organization group.
    /// </summary>
    /// <param name="organizationId">The identifier of the organization whose clients receive the notification.</param>
    /// <param name="monitor">The updated monitor data to send.</param>
    /// <param name="ct">The cancellation token for the asynchronous operation.</param>
    /// <returns>A task that represents the asynchronous notification operation.</returns>
    public async Task NotifyAsync(MonitorPollResult monitor, CancellationToken ct)
    {
        await _hubContext.Clients.Group(monitor.OrganizationId.ToString()).SendUpdatedMonitorsAsync([monitor], ct);
    }

    public async Task NotifyAsync(IReadOnlyCollection<MonitorPollResult> monitors, CancellationToken ct)
    {
        if (monitors.Count == 0)
        {
            return;
        }

        IEnumerable<IGrouping<Guid, MonitorPollResult>> orgMonitorsGroups = monitors.GroupBy(monitor => monitor.OrganizationId);
        foreach (IGrouping<Guid, MonitorPollResult> group in orgMonitorsGroups)
        {
            await _hubContext.Clients.Group(group.Key.ToString()).SendUpdatedMonitorsAsync(group.ToList(), ct);
        }
    }
}
