using Pulse.DAL.Commands.Monitors;

namespace Pulse.BL.Common.Notifications;

public interface INotificationService
{
    Task NotifyAsync(Guid organizationId, UpdateMonitorAfterPollInput update, CancellationToken ct);
    Task NotifyAsync(Guid organizationId, List<UpdateMonitorAfterPollInput> update, CancellationToken ct);
}
