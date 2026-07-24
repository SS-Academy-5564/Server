namespace Pulse.BL.Features.Polling.ManualTrigger;

/// <summary>
/// A bounded queue of monitor IDs awaiting a manually-triggered check.
/// </summary>
public interface IManualCheckQueue
{
    /// <summary>
    /// Attempts to enqueue a monitor check without blocking the caller.
    /// </summary>
    /// <returns><see langword="false"/> when the queue is full.</returns>
    bool TryEnqueue(Guid monitorId);

    ValueTask<Guid> DequeueAsync(CancellationToken ct);
}
