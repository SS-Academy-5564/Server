namespace Pulse.BL.Features.Polling.Http;

public sealed record HttpMonitorResponse(
    string RequestStatus,
    string? Body = null,
    bool IsSuccess = false,
    int? StatusCode = null,
    int ResponseTimeMs = 0,
    string? ErrorMessage = null
    );
