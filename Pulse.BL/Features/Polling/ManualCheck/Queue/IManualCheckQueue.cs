namespace Pulse.BL.Features.Polling.ManualCheck.Queue;

/// <summary>
/// A bounded queue of manually-triggered monitor checks.
/// </summary>
public interface IManualCheckQueue
{
    /// <summary>
    /// Attempts to enqueue a monitor check without blocking the caller.
    /// </summary>
    /// <param name="command">The manual check command to enqueue.</param>
    /// <returns><see langword="true"/> when the command is enqueued; otherwise, <see langword="false"/> when the queue is full.</returns>
    bool TryEnqueue(ManualCheckCommand command);

    /// <summary>
    /// Waits for and removes the next manual check command from the queue.
    /// </summary>
    /// <param name="ct">The cancellation token used to stop waiting for a command.</param>
    /// <returns>A value task containing the next queued manual check command.</returns>
    /// <exception cref="OperationCanceledException">Waiting for a command is canceled.</exception>
    ValueTask<ManualCheckCommand> DequeueAsync(CancellationToken ct);
}
