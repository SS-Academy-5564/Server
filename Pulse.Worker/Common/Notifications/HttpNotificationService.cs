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
    public Task NotifyAsync(UpdateMonitorAfterPollInput update, CancellationToken ct) => throw new NotImplementedException();
    public Task NotifyAsync(List<UpdateMonitorAfterPollInput> update, CancellationToken ct) => throw new NotImplementedException();
}
