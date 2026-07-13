namespace Pulse.DAL.Queries.Monitors;

public sealed record MonitorRecord(
    Guid Id,
    string Url,
    string HttpMethod,
    string ResultPath,
    int PollingIntervalSeconds,
    int PollingTimeoutSeconds
);
