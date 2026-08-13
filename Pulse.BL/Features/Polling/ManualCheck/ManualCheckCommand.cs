namespace Pulse.BL.Features.Polling.ManualCheck;

/// <summary>
/// Represents a request to queue a monitor for a manual check.
/// </summary>
/// <param name="MonitorId">The identifier of the monitor to check.</param>
/// <param name="OrganizationId">The identifier of the organization that owns the monitor.</param>
public sealed record ManualCheckCommand(Guid MonitorId, Guid OrganizationId);
