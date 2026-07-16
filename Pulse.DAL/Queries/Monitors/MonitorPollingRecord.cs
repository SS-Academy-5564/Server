namespace Pulse.DAL.Queries.Monitors;

public sealed record MonitorPollingRecord(
    Guid Id,
    string Url,
    string HttpMethod,
    string ResultPath,
    int PollingIntervalSeconds,
    int PollingTimeoutSeconds
);
