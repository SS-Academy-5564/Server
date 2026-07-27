namespace Pulse.BL.Features.Monitors.GetMonitorById;

public sealed record MonitorResult(
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
