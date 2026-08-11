using Pulse.BL.Features.Polling;

namespace Pulse.API.Hubs;

public interface INotificationClient
{
    /// <summary>
    /// Sends updated monitors data to a connected client.
    /// </summary>
    /// <param name="monitors">The updated monitors data.</param>
    /// <param name="ct">The cancellation token for the send operation.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    /// <exception cref="OperationCanceledException">The send operation is canceled.</exception>
    Task SendUpdatedMonitorsAsync(List<MonitorPollResult> monitors, CancellationToken ct);
}
