namespace Pulse.BL.Features.Polling.ManualCheck.Queue;

/// <summary>
/// A bounded queue of manually-triggered monitor checks.
/// </summary>
public interface IManualCheckQueue
{
    /// <summary>
    /// Attempts to enqueue a monitor check without blocking the caller.
    /// </summary>
    /// <returns><see langword="false"/> when the queue is full.</returns>
    bool TryEnqueue(ManualCheckJob job);

    ValueTask<ManualCheckJob> DequeueAsync(CancellationToken ct);
}
