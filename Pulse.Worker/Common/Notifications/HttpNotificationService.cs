using System.Net.Http.Json;
using Pulse.BL.Common.Notifications;
using Pulse.BL.Features.Polling;

namespace Pulse.Worker.Common.Notifications;

/// <summary>
/// Dispatches monitor notification updates using HTTP POST requests.
/// </summary>
public sealed class HttpNotificationService : IBatchMonitorNotificationService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpNotificationService"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client configured for sending notification payloads.</param>
    public HttpNotificationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Dispatches a batch of updated monitor polling results to the notification HTTP endpoint.
    /// </summary>
    /// <param name="updates">The batch of monitor poll results to send.</param>
    /// <param name="ct">The cancellation token for the HTTP request.</param>
    /// <returns>A task that represents the asynchronous HTTP notification operation.</returns>
    /// <exception cref="HttpRequestException">The HTTP request failed or returned a non-success status code.</exception>
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
