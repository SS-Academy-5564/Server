namespace Pulse.BL.Features.Polling.Http;

/// <summary>
/// The default values is assigned to reduce boilerplate code in the HttpMonitorClient
/// </summary>
/// <param name="RequestStatus">Must be one of the RequestStatusNames for correct work</param>
/// <param name="Body">The http response body</param>
/// <param name="IsSuccess">Is it the 200-299 status Code</param>
/// <param name="StatusCode">The Http StatusCode</param>
/// <param name="ResponseTimeMs"></param>
/// <param name="ErrorMessage"></param>
public sealed record HttpMonitorResponse(
    string RequestStatus,
    string? Body = null,
    bool IsSuccess = false,
    int? StatusCode = null,
    int ResponseTimeMs = 0,
    string? ErrorMessage = null
    );
