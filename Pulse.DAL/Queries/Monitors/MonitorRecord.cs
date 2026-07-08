namespace Pulse.DAL.Queries.Monitors;

public sealed record MonitorRecord(
    Guid Id,
    string Name,
    string Url,
    string? CurrentValue,
    DateTimeOffset? LastCheckedAt,
    MonitorStatus Status,
    int Interval);
