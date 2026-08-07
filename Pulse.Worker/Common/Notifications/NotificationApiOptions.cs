namespace Pulse.Worker.Common.Notifications;

public sealed class NotificationApiOptions
{
    public const string SectionName = "InternalNotifications";

    public string ApiBaseUrl { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
}
