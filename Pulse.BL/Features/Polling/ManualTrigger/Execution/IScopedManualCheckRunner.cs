namespace Pulse.BL.Features.Polling.ManualTrigger.Execution;

/// <summary>
/// Runs a single manual check inside its own DI scope, so each queued item
/// gets fresh scoped dependencies (DB connections, etc.) regardless of how
/// long it waited in the queue.
/// </summary>
public interface IScopedManualCheckRunner
{
    Task RunAsync(Guid monitorId, CancellationToken ct);
}
