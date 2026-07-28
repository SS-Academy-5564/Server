namespace Pulse.DAL.Commands.Monitors;

public sealed record CreateMonitorPollResultsInput(
    string? Value,
    bool IsSuccess,
    int ResponseTimeMs,
    int? StatusCode,
    Guid MonitorId,
    string RequestStatus);
