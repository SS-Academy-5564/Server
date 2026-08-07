using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Common.Notifications;
using Pulse.BL.Common.Notifications;
using Pulse.BL.Features.Polling;

namespace Pulse.API.Features.Internal.MonitorNotifications;

[ApiController]
[AllowAnonymous]
[Route(NotificationApiConstants.EndpointPath)]
[ServiceFilter<InternalNotificationApiKeyFilter>]
public sealed class MonitorNotificationsController : ControllerBase
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
