using FluentResults;

namespace Pulse.BL.Features.Polling.ManualTrigger;

/// <summary>
/// Defines operations for manually triggering monitor polling.
/// </summary>
public interface IManualMonitorTriggerService
{
    /// <summary>
    /// Triggers an immediate polling operation for the specified monitor.
    /// </summary>
    /// <param name="monitorId">The identifier of the monitor to poll.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>
    /// A successful result if the polling operation was scheduled or completed successfully;
    /// otherwise, a failed result describing the error.
    /// </returns>
    Task<Result> TriggerAsync(Guid monitorId, CancellationToken ct);
}