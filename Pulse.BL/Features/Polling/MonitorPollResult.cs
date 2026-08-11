namespace Pulse.BL.Features.Polling;

/// <summary>
/// Represents the updated monitor state produced by a polling operation.
/// </summary>
/// <param name="MonitorId">The identifier of the polled monitor.</param>
/// <param name="LastCheckedAt">The time when polling completed.</param>
/// <param name="NextExecutionAt">The time when the monitor should be polled again.</param>
/// <param name="Status">The monitor status after polling.</param>
public sealed record MonitorPollResult(
    Guid MonitorId,
    DateTime LastCheckedAt,
    DateTime NextExecutionAt,
    string Status,
    Guid OrganizationId)
{
    /// <summary>
    /// Gets the value extracted from the monitor response, or <c>null</c> when unavailable.
    /// </summary>
    public string? CurrentValue { get; init; }
}
