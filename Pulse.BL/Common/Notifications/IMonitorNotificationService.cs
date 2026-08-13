using Pulse.BL.Features.Polling;

namespace Pulse.BL.Common.Notifications;

/// <summary>
/// Service interface for dispatching a single monitor poll result notification.
/// </summary>
public interface IMonitorNotificationService
{
    /// <summary>
    /// Dispatches an updated monitor polling result.
    /// </summary>
    /// <param name="monitor">The monitor poll result to dispatch.</param>
    /// <param name="ct">The cancellation token for the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous notification dispatch.</returns>
    Task NotifyAsync(MonitorPollResult monitor, CancellationToken ct);
}
