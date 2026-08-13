namespace Pulse.DAL.Queries.Monitors;

/// <summary>
/// Represents a record of monitor configuration data required for polling operations.
/// </summary>
/// <param name="Id">The unique identifier of the monitor.</param>
/// <param name="Url">The target URL to perform the health check against.</param>
/// <param name="HttpMethod">The HTTP method used to send the request.</param>
/// <param name="ResultPath">The JSONPath expression used to extract status values from response bodies.</param>
/// <param name="PollingIntervalSeconds">The interval in seconds between polling cycles.</param>
/// <param name="PollingTimeoutSeconds">The timeout duration in seconds for HTTP requests.</param>
/// <param name="Status">The current status of the monitor.</param>
/// <param name="OrganizationId">The unique identifier of the organization that owns the monitor.</param>
public sealed record MonitorPollingRecord(
    Guid Id,
    string Url,
    string HttpMethod,
    string ResultPath,
    int PollingIntervalSeconds,
    int PollingTimeoutSeconds,
    string Status,
    Guid OrganizationId);
