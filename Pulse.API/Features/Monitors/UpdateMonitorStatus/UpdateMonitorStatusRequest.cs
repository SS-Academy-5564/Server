using Pulse.BL.Features.Monitors;

namespace Pulse.API.Features.Monitors.UpdateMonitorStatus;

/// <summary>
/// Represents a request to update a monitor's operational status.
/// </summary>
/// <param name="Status">The desired monitor status.</param>
public sealed record UpdateMonitorStatusRequest(MonitorStatus Status);
