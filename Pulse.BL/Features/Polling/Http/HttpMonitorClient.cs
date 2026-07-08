using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Pulse.DAL.Common.Constants;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.Polling.Http;

public sealed class HttpMonitorClient : IHttpMonitorClient
{
    public const string ClientName = "MonitorPolling";

    private static readonly HashSet<string> AllowedMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "GET",
        "POST",
        "PUT",
        "PATCH",
        "DELETE"
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HttpMonitorClient> _logger;

    public HttpMonitorClient(
        IHttpClientFactory httpClientFactory,
        ILogger<HttpMonitorClient> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<HttpMonitorResponse> SendAsync(MonitorRecord monitor, CancellationToken ct)
    {
        if (!AllowedMethods.Contains(monitor.HttpMethod))
        {
            return new HttpMonitorResponse(
                ErrorMessage: $"Unsupported HTTP method: {monitor.HttpMethod}",
                RequestStatus: RequestStatusNames.Failed
            );
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(monitor.PollingTimeoutSeconds));

        var stopwatch = Stopwatch.StartNew();

        try
        {
            using var httpClient = _httpClientFactory.CreateClient(ClientName);
            using var request = new HttpRequestMessage(new HttpMethod(monitor.HttpMethod), monitor.Url);

            using HttpResponseMessage response = await httpClient.SendAsync(request, timeoutCts.Token);
            string body = await response.Content.ReadAsStringAsync(timeoutCts.Token);

            return new HttpMonitorResponse(
                Body: body,
                IsSuccess: response.IsSuccessStatusCode,
                StatusCode: (int)response.StatusCode,
                ResponseTimeMs: (int)stopwatch.ElapsedMilliseconds,
                ErrorMessage: response.IsSuccessStatusCode ? null : response.ReasonPhrase,
                RequestStatus: response.IsSuccessStatusCode ? RequestStatusNames.Success : RequestStatusNames.Failed
            );
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return new HttpMonitorResponse(
                ResponseTimeMs: (int)stopwatch.ElapsedMilliseconds,
                ErrorMessage: "Request timed out.",
                RequestStatus: RequestStatusNames.Timeout
            );
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "Monitor HTTP request failed. MonitorId: {MonitorId}", monitor.Id);

            return new HttpMonitorResponse(
                ResponseTimeMs: (int)stopwatch.ElapsedMilliseconds,
                ErrorMessage: exception.Message,
                RequestStatus: RequestStatusNames.NetworkError
            );
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Monitor HTTP request failed. MonitorId: {MonitorId}", monitor.Id);

            return new HttpMonitorResponse(
                ResponseTimeMs: (int)stopwatch.ElapsedMilliseconds,
                ErrorMessage: exception.Message,
                RequestStatus: RequestStatusNames.UnexpectedError
            );
        }
    }
}
