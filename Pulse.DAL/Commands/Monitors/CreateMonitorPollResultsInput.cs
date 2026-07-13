namespace Pulse.DAL.Commands.Monitors;

public sealed record CreateMonitorPollResultsInput(
    string? Value,
    DateTime CheckedAt,
    bool IsSuccess,
    int ResponseTimeMs,
    int? StatusCode,
    Guid MonitorId,
    string RequestStatus);
