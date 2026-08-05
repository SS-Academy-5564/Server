using Pulse.BL.Features.Polling;

namespace Pulse.API.Hubs;

public interface INotificationClient
{
    Task SendUpdatedMonitorAsync(MonitorPollResult monitor);
    Task SendUpdatedMonitorsAsync(List<MonitorPollResult> monitors);
}
