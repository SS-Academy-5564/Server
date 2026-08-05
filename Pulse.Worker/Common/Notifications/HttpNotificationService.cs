using Pulse.BL.Common.Notifications;
using Pulse.BL.Features.Polling;

namespace Pulse.Worker.Common.Notifications;

public class HttpNotificationService : IBatchMonitorNotificationService
{

    private readonly IHttpClientFactory _httpClientFactory;

    public HttpNotificationService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public Task NotifyAsync(IReadOnlyCollection<MonitorPollResult> update, CancellationToken ct)
    {
        using HttpClient client = _httpClientFactory.CreateClient();

        return Task.CompletedTask;
    }
}
