namespace Pulse.BL.Features.Polling.ManualCheck;

/// <summary>
/// Represents a request to queue a monitor for a manual check.
/// </summary>
public sealed record ManualCheckCommand(Guid MonitorId, Guid OrganizationId);
