namespace Pulse.API.Common.Notifications;

public sealed class InternalNotificationOptions
{
    public const string SectionName = "InternalNotifications";

    public required string ApiKey { get; init; }
}
