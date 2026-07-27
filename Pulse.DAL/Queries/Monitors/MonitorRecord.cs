namespace Pulse.DAL.Queries.Monitors;

public sealed record MonitorRecord(
    Guid Id,
    string Name,
    string Url,
    string HttpMethod,
    string ResultPath,
    string? CurrentValue,
    MonitorStatus Status,
    int PollingIntervalSeconds,
    int PollingTimeoutSeconds,
    DateTimeOffset LastCheckedAt,
    DateTime NextExecutionAt);
