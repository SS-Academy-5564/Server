namespace Pulse.BL.Features.Monitors.UpdateMonitor;

public sealed record UpdateMonitorCommand(
    Guid Id,
    string Name,
    string Url,
    string HttpMethod,
    string ResultPath,
    MonitorStatus Status,
    int PollingIntervalSeconds,
    int PollingTimeoutSeconds);
