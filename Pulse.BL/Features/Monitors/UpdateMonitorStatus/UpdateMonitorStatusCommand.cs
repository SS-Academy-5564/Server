namespace Pulse.BL.Features.Monitors.UpdateMonitorStatus;

public sealed record UpdateMonitorStatusCommand(
    Guid MonitorId,
    MonitorStatus Status);
