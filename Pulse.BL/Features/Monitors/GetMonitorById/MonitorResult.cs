namespace Pulse.BL.Features.Monitors.GetMonitorById;

public sealed record MonitorResult(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string Url,
    string HttpMethod,
    string ResultPath,
    string? CurrentValue,
    MonitorStatus Status,
    int PollingIntervalSeconds,
    int PollingTimeoutSeconds,
    DateTimeOffset LastCheckedAt,
    DateTime NextExecutionAt,
    DateTimeOffset CreatedAt,
    DateTimeOffset LastModifiedAt);
