namespace Pulse.Worker.Common.Notifications;

/// <summary>
/// Configuration options for internal notification API settings in the worker.
/// </summary>
public sealed class NotificationApiOptions
{
    /// <summary>
    /// The configuration section name for internal notifications.
    /// </summary>
    public const string SectionName = "InternalNotifications";

    /// <summary>
    /// Gets the base URL of the notification API endpoint.
    /// </summary>
    public required string ApiBaseUrl { get; init; }

    /// <summary>
    /// Gets the API key used to authenticate requests to the notification API.
    /// </summary>
    public required string ApiKey { get; init; }
}
