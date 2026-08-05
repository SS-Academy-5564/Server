using Pulse.BL.Common.Notifications;
using Pulse.BL.Features.Polling;

namespace Pulse.Worker.Common.Notifications;

public class HttpNotificationService : INotificationService
{

    private readonly IHttpClientFactory _httpClientFactory;

    public HttpNotificationService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task NotifyAsync(MonitorPollResult update, CancellationToken ct)
    {
    }

    public Task NotifyAsync(List<MonitorPollResult> update, CancellationToken ct)
    {
        using var httpClient = _httpClientFactory.CreateClient();
    }
}
