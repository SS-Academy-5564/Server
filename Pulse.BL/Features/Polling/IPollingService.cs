using FluentResults;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.Polling;

public interface IPollingService
{
    /// <summary>
    /// Retrieves enabled monitors that are due for polling up to the specified maximum record count.
    /// </summary>
    /// <param name="numberOfRecords">The maximum number of records to retrieve.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>A result containing due monitor polling records.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled.</exception>
    Task<Result<IEnumerable<MonitorPollingRecord>>> GetDueEnabledAsync(int numberOfRecords, CancellationToken ct);

    /// <summary>
    /// Processes the supplied monitor polling record.
    /// </summary>
    /// <param name="monitor">The monitor polling record to process.</param>
    /// <param name="ct">The cancellation token for the polling operation.</param>
    /// <returns>A result containing the completed monitor polling data, or an error if polling fails.</returns>
    /// <exception cref="OperationCanceledException">The polling operation is canceled.</exception>
    Task<Result<MonitorPollResult>> ProcessMonitorAsync(MonitorPollingRecord monitor, CancellationToken ct);

    /// <summary>
    /// Finds and processes a monitor belonging to the specified organization.
    /// </summary>
    /// <param name="monitorId">The identifier of the monitor to process.</param>
    /// <param name="organizationId">The identifier of the monitor's organization.</param>
    /// <param name="ct">The cancellation token for the polling operation.</param>
    /// <returns>A result containing the completed monitor polling data, or an error if the monitor cannot be processed.</returns>
    /// <exception cref="OperationCanceledException">The polling operation is canceled.</exception>
    Task<Result<MonitorPollResult>> ProcessMonitorAsync(Guid monitorId, Guid organizationId, CancellationToken ct);
}
