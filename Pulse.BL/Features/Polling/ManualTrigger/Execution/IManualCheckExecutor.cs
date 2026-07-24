namespace Pulse.BL.Features.Polling.ManualTrigger.Execution;

/// <summary>
/// Executes a single queued manual monitor check. Resolved per work item
/// from a fresh DI scope so its dependencies are never reused across items.
/// </summary>
public interface IManualCheckExecutor
{
    Task ExecuteAsync(Guid monitorId, CancellationToken ct);
}
