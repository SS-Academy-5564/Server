using System.Net.Http.Json;
using Pulse.BL.Common.Notifications;
using Pulse.BL.Features.Polling;

namespace Pulse.Worker.Common.Notifications;

public sealed class HttpNotificationService : IBatchMonitorNotificationService
{
    private readonly HttpClient _httpClient;

    public HttpNotificationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task NotifyAsync(IReadOnlyCollection<MonitorPollResult> updates, CancellationToken ct)
    {
        if (updates.Count == 0)
        {
            return;
        }

        using HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
            NotificationApiConstants.EndpointPath,
            updates,
            ct);
        response.EnsureSuccessStatusCode();
    }
}
