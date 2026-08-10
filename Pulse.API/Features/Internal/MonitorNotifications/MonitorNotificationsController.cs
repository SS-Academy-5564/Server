using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Controllers;
using Pulse.API.Filters.InternalNotificatiom;
using Pulse.BL.Common.Notifications;
using Pulse.BL.Features.Polling;

namespace Pulse.API.Features.Internal.MonitorNotifications;

[AllowAnonymous]
[Route(NotificationApiConstants.EndpointPath)]
[ServiceFilter<InternalNotificationApiKeyFilter>]
public sealed class MonitorNotificationsController : PulseControllerBase
{
    private readonly IBatchMonitorNotificationService _notificationService;

    public MonitorNotificationsController(IBatchMonitorNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    [HttpPost]
    public async Task<IActionResult> NotifyAsync(
        [FromBody] IReadOnlyCollection<MonitorPollResult> monitors,
        CancellationToken ct)
    {
        await _notificationService.NotifyAsync(monitors, ct);
        return NoContent();
    }
}
