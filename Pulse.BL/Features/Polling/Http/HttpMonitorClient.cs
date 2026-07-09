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
            _logger.LogWarning(
                "Unsupported HTTP method configured for monitor. MonitorId: {MonitorId}, HttpMethod: {HttpMethod}",
                monitor.Id,
                monitor.HttpMethod);

            return new HttpMonitorResponse(
                IsSuccess: false,
                ResponseTimeMs: 0,
                RequestStatus: RequestStatusNames.Failed);
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

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Monitor HTTP request completed with non-success status. MonitorId: {MonitorId}, StatusCode: {StatusCode}, ReasonPhrase: {ReasonPhrase}, ResponseTimeMs: {ResponseTimeMs}",
                    monitor.Id,
                    (int)response.StatusCode,
                    response.ReasonPhrase,
                    stopwatch.ElapsedMilliseconds);
            }

            return new HttpMonitorResponse(
                IsSuccess: response.IsSuccessStatusCode,
                ResponseTimeMs: (int)stopwatch.ElapsedMilliseconds,
                RequestStatus: response.IsSuccessStatusCode ? RequestStatusNames.Success : RequestStatusNames.Failed
            ) { Body = body, StatusCode = (int?)response.StatusCode, };
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Monitor HTTP request timed out. MonitorId: {MonitorId}, TimeoutSeconds: {TimeoutSeconds}, ResponseTimeMs: {ResponseTimeMs}",
                monitor.Id,
                monitor.PollingTimeoutSeconds,
                stopwatch.ElapsedMilliseconds);

            return new HttpMonitorResponse(
                IsSuccess: false,
                ResponseTimeMs: (int)stopwatch.ElapsedMilliseconds,
                RequestStatus: RequestStatusNames.Timeout);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "Monitor HTTP request failed. MonitorId: {MonitorId}", monitor.Id);

            return new HttpMonitorResponse(
                IsSuccess: false,
                ResponseTimeMs: (int)stopwatch.ElapsedMilliseconds,
                RequestStatus: RequestStatusNames.NetworkError);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Monitor HTTP request failed. MonitorId: {MonitorId}", monitor.Id);

            return new HttpMonitorResponse(
                IsSuccess: false,
                ResponseTimeMs: (int)stopwatch.ElapsedMilliseconds,
                RequestStatus: RequestStatusNames.UnexpectedError);
        }
    }
}
