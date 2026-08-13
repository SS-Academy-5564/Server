using Pulse.BL.Features.Polling;

namespace Pulse.BL.Common.Notifications;

/// <summary>
/// Service interface for dispatching batch notifications of monitor poll results.
/// </summary>
public interface IBatchMonitorNotificationService
{
    /// <summary>
    /// Dispatches a batch of updated monitor polling results.
    /// </summary>
    /// <param name="monitors">The batch of monitor poll results to dispatch.</param>
    /// <param name="ct">The cancellation token for the asynchronous operation.</param>
    /// <returns>A task representing the asynchronous notification dispatch.</returns>
    Task NotifyAsync(IReadOnlyCollection<MonitorPollResult> monitors, CancellationToken ct);
}
