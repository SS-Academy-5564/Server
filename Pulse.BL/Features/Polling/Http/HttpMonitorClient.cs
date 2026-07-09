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
                IsSuccess: false,
                ResponseTimeMs: 0,
                RequestStatus: RequestStatusNames.Failed
            )
            {
                ErrorMessage = $"Unsupported HTTP method: {monitor.HttpMethod}",
            };
        }

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(monitor.PollingTimeoutSeconds));

        var stopwatch = Stopwatch.StartNew();

        try
        {
            using HttpClient httpClient = _httpClientFactory.CreateClient(ClientName);
            using HttpRequestMessage request = new(new HttpMethod(monitor.HttpMethod), monitor.Url);

            using HttpResponseMessage response = await httpClient.SendAsync(request, timeoutCts.Token);
            string body = await response.Content.ReadAsStringAsync(timeoutCts.Token);

            return new HttpMonitorResponse(
                IsSuccess: response.IsSuccessStatusCode,
                ResponseTimeMs: (int)stopwatch.ElapsedMilliseconds,
                RequestStatus: response.IsSuccessStatusCode ? RequestStatusNames.Success : RequestStatusNames.Failed
            )
            {
                Body = body,
                StatusCode = (int?)response.StatusCode,
                ErrorMessage = response.IsSuccessStatusCode ? null : response.ReasonPhrase,
            };
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            return new HttpMonitorResponse(
                IsSuccess: false,
                ResponseTimeMs: (int)stopwatch.ElapsedMilliseconds,
                RequestStatus: RequestStatusNames.Timeout
            )
            {
                ErrorMessage= "Request timed out.",
            };
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "Monitor HTTP request failed. MonitorId: {MonitorId}", monitor.Id);

            return new HttpMonitorResponse(
                IsSuccess: false,
                ResponseTimeMs: (int)stopwatch.ElapsedMilliseconds,
                RequestStatus: RequestStatusNames.NetworkError
            )
            {
                ErrorMessage = exception.Message,
            };
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Monitor HTTP request failed. MonitorId: {MonitorId}", monitor.Id);

            return new HttpMonitorResponse(
                IsSuccess: false,
                ResponseTimeMs: (int)stopwatch.ElapsedMilliseconds,
                RequestStatus: RequestStatusNames.UnexpectedError
            )
            {
                ErrorMessage= exception.Message,
            };
        }
    }
}
