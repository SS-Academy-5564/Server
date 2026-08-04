using Pulse.BL.Features.Monitors;

namespace Pulse.API.Hubs;

public interface INotificationClient
{
    /// <summary>
    /// Will be called by the Poller Worker
    /// </summary>
    /// <param name="monitors"></param>
    Task SendUpdatedMonitorsAsync(List<MonitorListResult> monitors);
    Task SendUpdatedMonitorAsync(MonitorListResult monitor);
}
