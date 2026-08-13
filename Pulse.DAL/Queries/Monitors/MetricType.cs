namespace Pulse.DAL.Queries.Monitors;

/// <summary>
/// Represents the category of metric to retrieve for a monitor's poll-result history.
/// </summary>
public enum MetricType
{
    /// <summary>Response time in seconds per poll result.</summary>
    ResponseTime,

    /// <summary>Percentage of successful polls over the requested time window.</summary>
    Availability,

    /// <summary>Total number of poll requests over the requested time window.</summary>
    Requests,

    /// <summary>Number of failed poll requests over the requested time window.</summary>
    Errors,
}
