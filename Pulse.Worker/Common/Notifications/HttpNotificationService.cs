using Pulse.BL.Common.Notifications;
using Pulse.DAL.Commands.Monitors;

namespace Pulse.Worker.Common.Notifications;

public class HttpNotificationService : INotificationService
{
    private readonly HttpClient _httpClient;
    public HttpNotificationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    //send to the api layer
    // TODO
    public Task NotifyAsync(Guid organizationId, UpdateMonitorAfterPollInput update, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
    public Task NotifyAsync(Guid organizationId,List<UpdateMonitorAfterPollInput> update, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
