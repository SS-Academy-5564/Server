namespace Pulse.API.Filters.InternalNotificatiom;

/// <summary>
/// Configuration options for internal notification services.
/// </summary>
public sealed class InternalNotificationOptions
{
    /// <summary>
    /// The configuration section name for internal notifications.
    /// </summary>
    public const string SectionName = "InternalNotifications";

    /// <summary>
    /// Gets the API key required for internal notification authorization.
    /// </summary>
    public required string ApiKey { get; init; }
}
