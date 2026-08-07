namespace Pulse.Worker.Common.Notifications;

public sealed class NotificationApiOptions
{
    public const string SectionName = "InternalNotifications";

    public required string ApiBaseUrl { get; init; }
    public required string ApiKey { get; init; }
}
