using System.Text.Json;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Helpers.Json;
using Pulse.BL.Features.Polling.Http;
using Pulse.BL.Features.Polling.Options;
using Pulse.DAL.Commands.MonitorPollResults;
using Pulse.DAL.Commands.Monitors;
using Pulse.DAL.Common.Constants;
using Pulse.DAL.Common.Repository;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.Polling;

public class PollingService : IPollingService
{
    private readonly ILogger<PollingService> _logger;
    private readonly PollingWorkerOptions _options;
    private readonly IMonitorQueries _monitorQueries;
    private readonly IMonitorCommands _monitorCommands;
    private readonly IMonitorPollResultsCommands _monitorPollResultCommands;
    private readonly IHttpMonitorClient _httpMonitorClient;
    private readonly IJsonPathReader _jsonPathReader;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public PollingService(
        ILogger<PollingService> logger,
        IOptions<PollingWorkerOptions> options,
        IHttpMonitorClient httpMonitorClient,
        IJsonPathReader jsonPathReader,
        IMonitorQueries monitorQueries,
        IMonitorCommands monitorCommands,
        IMonitorPollResultsCommands monitorPollResultCommands,
        IUnitOfWorkFactory unitOfWorkFactory)
    {
        _logger = logger;
        _options = options.Value;
        _httpMonitorClient = httpMonitorClient;
        _jsonPathReader = jsonPathReader;
        _monitorQueries = monitorQueries;
        _monitorCommands = monitorCommands;
        _unitOfWorkFactory = unitOfWorkFactory;
        _monitorPollResultCommands = monitorPollResultCommands;
    }

    public async Task<Result> ProcessDueMonitorsAsync(CancellationToken ct = default)
    {
        IEnumerable<MonitorRecord> monitors = await _monitorQueries.GetDueEnabledAsync(_options.BatchSize, ct);

        ParallelOptions options = new()
        {
            MaxDegreeOfParallelism = _options.MaxConcurrentRequests,
            CancellationToken = ct
        };

        await Parallel.ForEachAsync(monitors, options, async (monitor, token) =>
        {
            try
            {
                await ProcessMonitorAsync(monitor, token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to process monitor. MonitorId: {MonitorId}",
                    monitor.Id);
            }
        });

        return Result.Ok();
    }

    /// <summary>
    /// Executes one monitor check and persists the resulting poll record.
    /// </summary>
    /// <remarks>
    /// HTTP/extraction failures become poll-result statuses when possible. Worker cancellation is rethrown.
    /// </remarks>
    private async ValueTask ProcessMonitorAsync(MonitorRecord monitor, CancellationToken ct)
    {
        string? value = null;
        CreateMonitorPollResultsInput resultInput;

        try
        {
            HttpMonitorResponse response = await _httpMonitorClient.SendAsync(monitor, ct);
            bool isSuccess = response.IsSuccess;
            string requestStatus = response.RequestStatus;

            if (response.IsSuccess && !string.IsNullOrWhiteSpace(response.Body))
            {
                try
                {
                    value = _jsonPathReader.ReadValue(response.Body, monitor.ResultPath);
                }
                catch (Exception exception) when (exception is JsonException or InvalidOperationException or ArgumentException)
                {
                    _logger.LogWarning(
                        exception,
                        "Failed to extract monitor value. MonitorId: {MonitorId}, ResultPath: {ResultPath}",
                        monitor.Id,
                        monitor.ResultPath);

                    isSuccess = false;
                    requestStatus = RequestStatusNames.ExtractionError;
                }
            }

            resultInput = new(
                value,
                DateTime.UtcNow,
                isSuccess,
                response.ResponseTimeMs,
                response.StatusCode,
                monitor.Id,
                requestStatus);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected monitor polling error. MonitorId: {MonitorId}", monitor.Id);

            resultInput = new(
                Value: null,
                CheckedAt: DateTime.UtcNow,
                IsSuccess: false,
                ResponseTimeMs: 0,
                StatusCode: null,
                MonitorId: monitor.Id,
                RequestStatus: RequestStatusNames.UnexpectedError);
        }

        DateTime completedAt = DateTime.UtcNow;
        DateTime nextExecutionAt = completedAt.AddSeconds(monitor.PollingIntervalSeconds);

        UpdateMonitorAfterPollInput monitorInput = new(
            monitor.Id,
            value,
            completedAt,
            nextExecutionAt);

        await using IUnitOfWork uof = await _unitOfWorkFactory.CreateAsync(ct: ct);
        await _monitorPollResultCommands.CreateAsync(resultInput, ct);
        await _monitorCommands.UpdateAfterPollAsync(monitorInput, ct);
        await uof.CommitAsync(ct);
    }
}
