using Microsoft.AspNetCore.SignalR;
using Pulse.API.Hubs;
using Pulse.BL.Common.Notifications;
using Pulse.BL.Common.Security;
using Pulse.DAL.Commands.Monitors;
using Pulse.DAL.Common.Constants;

namespace Pulse.API.Common.Notifications;

public class SignalrNotificationService : INotificationService
{
    private readonly IHubContext<PulseNotificationHub, INotificationClient> _hubContext;
    private readonly ICurrentUserService _userService;

    public SignalrNotificationService(IHubContext<PulseNotificationHub, INotificationClient> hubContext, ICurrentUserService userService)
    {
        _hubContext = hubContext;
        _userService = userService;
    }

    public async Task NotifyAsync(Guid organizationId, UpdateMonitorAfterPollInput monitor, CancellationToken ct)
    {
        await _hubContext.Clients.Group(organizationId.ToString()).SendUpdatedMonitorAsync(monitor);
    }

    public async Task NotifyAsync(List<UpdateMonitorAfterPollInput> monitors, CancellationToken ct)
    {
        Guid organizationId = _userService.OrganizationId ?? SeededIds.Organizations.Default;
        await _hubContext.Clients.Groups(organizationId.ToString()).SendUpdatedMonitorsAsync(monitors);
    }
}
