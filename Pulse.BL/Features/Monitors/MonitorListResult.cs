namespace Pulse.BL.Features.Monitors;

/// <summary>
/// Represents the result of listing a monitor.
/// </summary>
/// <param name="Id">The unique identifier of the monitor.</param>
/// <param name="Name">The display name of the monitor.</param>
/// <param name="Url">The endpoint URL the monitor polls.</param>
/// <param name="CurrentValue">The most recently extracted value from the last successful poll, or <c>null</c> if no value has been recorded.</param>
/// <param name="LastCheckedAt">The timestamp of the last poll, or <c>null</c> if the monitor has not yet been checked.</param>
/// <param name="Status">The current status of the monitor.</param>
/// <param name="Interval">The polling interval in seconds.</param>
/// <param name="OrganizationId">The organization that owns this monitor.</param>
public sealed record MonitorListResult(
    Guid Id,
    string Name,
    string Url,
    string? CurrentValue,
    DateTimeOffset? LastCheckedAt,
    MonitorStatus Status,
    int Interval,
    Guid OrganizationId);
