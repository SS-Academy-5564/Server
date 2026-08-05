using Pulse.BL.Features.Polling;

namespace Pulse.API.Hubs;

public interface INotificationClient
{
    Task SendUpdatedMonitorAsync(MonitorPollResult monitor);
}
