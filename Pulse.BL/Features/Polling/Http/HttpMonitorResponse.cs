namespace Pulse.BL.Features.Polling.Http;

public sealed record HttpMonitorResponse(
    bool IsSuccess,
    int ResponseTimeMs,
    string RequestStatus)
{
    public string? Body { get; set; }
    public int? StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
}
