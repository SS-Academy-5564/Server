namespace Pulse.DAL.Commands.Monitors;

/// <summary>
/// Represents the input required to create a new monitor.
/// </summary>
/// <param name="Name">The display name of the monitor.</param>
/// <param name="Url">The endpoint URL to poll.</param>
/// <param name="HttpMethod">The HTTP method used to send the request.</param>
/// <param name="ResultPath">The JSON path used to extract the monitored value from the response.</param>
/// <param name="PollingIntervalSeconds">The interval in seconds between successive polls.</param>
/// <param name="PollingTimeoutSeconds">The timeout in seconds to wait for a response before treating the poll as failed.</param>
/// <param name="OrganizationId">The organization that owns this monitor.</param>
public sealed record CreateMonitorInput(
    string Name,
    string Url,
    string HttpMethod,
    string ResultPath,
    int PollingIntervalSeconds,
    int PollingTimeoutSeconds,
    Guid OrganizationId);
