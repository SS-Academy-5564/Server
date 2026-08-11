namespace Pulse.BL.Common.Notifications;

/// <summary>
/// Constants used for internal notification API communication.
/// </summary>
public static class NotificationApiConstants
{
    /// <summary>
    /// The relative endpoint path for monitor notifications.
    /// </summary>
    public const string EndpointPath = "api/internal/monitor-notifications";

    /// <summary>
    /// The HTTP header name used for internal API key authentication.
    /// </summary>
    public const string ApiKeyHeaderName = "X-Internal-Api-Key";
}
