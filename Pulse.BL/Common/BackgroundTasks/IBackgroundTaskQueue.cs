namespace Pulse.BL.Common.BackgroundTasks;

/// <summary>
/// Defines a queue for scheduling background work items to be processed asynchronously.
/// </summary>
public interface IBackgroundTaskQueue
{
    /// <summary>
    /// Enqueues a background work item for later execution.
    /// </summary>
    /// <param name="workItem">
    /// A delegate representing the background operation. The delegate receives
    /// an <see cref="IServiceProvider"/> for resolving scoped services and a
    /// <see cref="CancellationToken"/> for observing cancellation requests.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous enqueue operation.
    /// </returns>
    ValueTask EnqueueAsync(Func<IServiceProvider, CancellationToken, Task> workItem);

    /// <summary>
    /// Dequeues the next background work item, waiting until one becomes available if necessary.
    /// </summary>
    /// <param name="ct">A token to cancel the wait for a queued work item.</param>
    /// <returns>
    /// A delegate representing the next background operation to execute.
    /// The delegate receives an <see cref="IServiceProvider"/> for resolving
    /// scoped services and a <see cref="CancellationToken"/> for observing
    /// cancellation requests during execution.
    /// </returns>
    ValueTask<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken ct);
}