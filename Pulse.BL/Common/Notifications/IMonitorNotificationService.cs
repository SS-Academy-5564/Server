using Pulse.BL.Features.Polling;

namespace Pulse.BL.Common.Notifications;

public interface IMonitorNotificationService
{
    Task NotifyAsync(MonitorPollResult monitor, CancellationToken ct);
}
