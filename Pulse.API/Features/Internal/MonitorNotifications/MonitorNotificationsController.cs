using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Controllers;
using Pulse.API.Filters.InternalNotification;
using Pulse.BL.Common.Notifications;
using Pulse.BL.Features.Polling;

namespace Pulse.API.Features.Internal.MonitorNotifications;

/// <summary>
/// Handles internal HTTP notifications for monitor polling updates.
/// </summary>
[AllowAnonymous]
[Route(NotificationApiConstants.EndpointPath)]
[ServiceFilter<InternalNotificationApiKeyFilter>]
public sealed class MonitorNotificationsController : PulseControllerBase
{
    private readonly IBatchMonitorNotificationService _notificationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="MonitorNotificationsController"/> class.
    /// </summary>
    /// <param name="notificationService">The batch notification service for dispatching monitor updates.</param>
    public MonitorNotificationsController(IBatchMonitorNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    /// <summary>
    /// Receives a batch of updated monitor polling results and forwards them to the notification service.
    /// </summary>
    /// <param name="monitors">The batch of updated monitor poll results.</param>
    /// <param name="ct">The cancellation token for the request.</param>
    /// <returns>An HTTP response indicating the notifications were processed.</returns>
    [HttpPost]
    public async Task<IActionResult> NotifyAsync(
        [FromBody] IReadOnlyCollection<MonitorPollResult> monitors,
        CancellationToken ct)
    {
        await _notificationService.NotifyAsync(monitors, ct);
        return NoContent();
    }
}
