using Pulse.BL.Features.Monitors;

namespace Pulse.API.Features.Monitors.UpdateMonitor;

public sealed record UpdateMonitorRequest(
    string Name,
    string Url,
    string HttpMethod,
    string ResultPath,
    MonitorStatus Status,
    int PollingIntervalSeconds,
    int PollingTimeoutSeconds);
