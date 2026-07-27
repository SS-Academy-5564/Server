namespace Pulse.BL.Features.Monitors;

public sealed record MonitorListResult(
    Guid Id,
    string Name,
    string Url,
    string? CurrentValue,
    DateTimeOffset? LastCheckedAt,
    MonitorStatus Status,
    int Interval);

