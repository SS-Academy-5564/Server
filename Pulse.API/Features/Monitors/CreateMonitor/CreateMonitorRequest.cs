namespace Pulse.API.Features.Monitors.CreateMonitor;

/// <summary>
/// Represents a request to create a new monitor.
/// </summary>
/// <param name="Name">The monitor name.</param>
/// <param name="Url">The endpoint URL to poll.</param>
/// <param name="HttpMethod">The HTTP method to use for polling.</param>
/// <param name="ResultPath">The JSON path for extracting the monitor result.</param>
/// <param name="PollingIntervalSeconds">The interval between polls, in seconds.</param>
/// <param name="PollingTimeoutSeconds">The timeout for each poll, in seconds.</param>
public sealed record CreateMonitorRequest(
    string Name,
    string Url,
    string HttpMethod,
    string ResultPath,
    int PollingIntervalSeconds,
    int PollingTimeoutSeconds);
