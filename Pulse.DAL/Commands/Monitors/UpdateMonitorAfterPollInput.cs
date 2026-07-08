namespace Pulse.DAL.Commands.Monitors;

public sealed record UpdateMonitorAfterPollInput(
    Guid MonitorId,
    string? CurrentValue,
    DateTime LastCheckedAt,
    DateTime NextExecutionAt);
