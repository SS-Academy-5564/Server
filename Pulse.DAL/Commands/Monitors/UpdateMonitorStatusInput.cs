namespace Pulse.DAL.Commands.Monitors;

public sealed record UpdateMonitorStatusInput(
    Guid MonitorId,
    Guid OrganizationId,
    string Status);
