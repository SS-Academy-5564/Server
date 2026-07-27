namespace Pulse.DAL.Queries.Monitors;

public sealed record MonitorListRecord(
    Guid Id,
    string Name,
    string Url,
    string? CurrentValue,
    DateTimeOffset? LastCheckedAt,
    MonitorStatus Status,
    int Interval);
