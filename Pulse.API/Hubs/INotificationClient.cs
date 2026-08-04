using Pulse.DAL.Commands.Monitors;

namespace Pulse.API.Hubs;

public interface INotificationClient
{
    /// <summary>
    /// Will be called by the Poller Worker
    /// </summary>
    /// <param name="monitors"></param>
    Task SendUpdatedMonitorsAsync(List<UpdateMonitorAfterPollInput> monitors);
    Task SendUpdatedMonitorAsync(UpdateMonitorAfterPollInput monitor);
}
