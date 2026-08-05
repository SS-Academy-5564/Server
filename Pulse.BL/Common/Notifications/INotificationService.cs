using Pulse.BL.Features.Polling;

namespace Pulse.BL.Common.Notifications;

public interface INotificationService
{
    Task NotifyAsync(MonitorPollResult update, CancellationToken ct);
    Task NotifyAsync(List<MonitorPollResult> update, CancellationToken ct);
}
