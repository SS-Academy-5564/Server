namespace Pulse.BL.Features.Monitors;

public sealed record MonitorResult(
    Guid Id,
    string Name,
    string Url,
    string? CurrentValue,
    DateTimeOffset? LastCheckedAt,
    MonitorStatus Status,
    int Interval);

