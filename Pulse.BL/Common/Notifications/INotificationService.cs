using Pulse.DAL.Commands.Monitors;

namespace Pulse.BL.Common.Notifications;

public interface INotificationService
{
    Task NotifyAsync(UpdateMonitorAfterPollInput update, CancellationToken ct);
    Task NotifyAsync(List<UpdateMonitorAfterPollInput> update, CancellationToken ct);
}
