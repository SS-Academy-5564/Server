namespace Pulse.DAL.Commands.Monitors;

public sealed record UpdateMonitorInput(
    Guid Id,
    string Name,
    string Url,
    string HttpMethod,
    string ResultPath,
    string Status,
    int PollingIntervalSeconds,
    int PollingTimeoutSeconds);
