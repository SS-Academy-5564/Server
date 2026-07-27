namespace Pulse.DAL.Commands.Monitors;

public sealed record CreateMonitorInput(
    string Name,
    string Url,
    string HttpMethod,
    string ResultPath,
    int PollingIntervalSeconds,
    int PollingTimeoutSeconds);
