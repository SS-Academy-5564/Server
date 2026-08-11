namespace Pulse.API.Filters.InternalNotificatiom;

public sealed class InternalNotificationOptions
{
    public const string SectionName = "InternalNotifications";

    public required string ApiKey { get; init; }
}
