using Pulse.BL.Features.Polling;

namespace Pulse.API.Hubs;

public interface INotificationClient
{
    /// <summary>
    /// Sends updated monitor data to a connected client.
    /// </summary>
    /// <param name="monitor">The updated monitor data.</param>
    /// <param name="ct">The cancellation token for the send operation.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    /// <exception cref="OperationCanceledException">The send operation is canceled.</exception>
    Task SendUpdatedMonitorAsync(MonitorPollResult monitor, CancellationToken ct);

    Task SendUpdatedMonitorsAsync(List<MonitorPollResult> monitors, CancellationToken ct);
}
