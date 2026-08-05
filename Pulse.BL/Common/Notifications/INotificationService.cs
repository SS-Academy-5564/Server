using Pulse.BL.Features.Polling;

namespace Pulse.BL.Common.Notifications;

public interface INotificationService
{
    /// <summary>
    /// Notifies an organization's connected clients about updated monitor data.
    /// </summary>
    /// <param name="organizationId">The identifier of the organization whose clients receive the update.</param>
    /// <param name="update">The updated monitor data.</param>
    /// <param name="ct">The cancellation token for the notification operation.</param>
    /// <returns>A task that represents the asynchronous notification operation.</returns>
    /// <exception cref="OperationCanceledException">The notification operation is canceled.</exception>
    Task NotifyAsync(Guid organizationId, MonitorPollResult update, CancellationToken ct);
}
