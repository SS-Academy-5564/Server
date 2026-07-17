namespace Pulse.API.Features.Monitors.CreateMonitor;

public sealed record CreateMonitorRequest(
    string Name,
    string Url,
    string HttpMethod,
    string ResultPath,
    int PollingIntervalSeconds,
    int PollingTimeoutSeconds);
