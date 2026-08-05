using Pulse.BL.Features.Polling;

namespace Pulse.BL.Common.Notifications;

public interface IBatchMonitorNotificationService
{
    Task NotifyAsync(IReadOnlyCollection<MonitorPollResult> monitors, CancellationToken ct);
}
