using Pulse.BL.Features.Polling;

namespace Pulse.BL.Common.Notifications;

public interface INotificationService
{
    Task NotifyAsync(Guid organizationId, MonitorPollResult update, CancellationToken ct);
}
