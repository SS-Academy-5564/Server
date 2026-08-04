using Pulse.BL.Features.Monitors;

namespace Pulse.BL.Common.Notifications;

public interface INotificationService
{
    Task NotifyAsync(MonitorListResult update, CancellationToken ct);
    Task NotifyAsync(List<MonitorListResult> update, CancellationToken ct);
}
