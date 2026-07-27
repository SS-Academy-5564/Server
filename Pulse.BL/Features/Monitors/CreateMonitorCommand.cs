namespace Pulse.BL.Features.Monitors;

public sealed record CreateMonitorCommand(
    string Name,
    string Url,
    string HttpMethod,
    string ResultPath,
    int PollingIntervalSeconds,
    int PollingTimeoutSeconds);
