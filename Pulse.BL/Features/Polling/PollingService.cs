using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Errors;
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

    /// <summary>
    /// Processes the enabled monitors that are due for polling.
    /// </summary>
    /// <param name="ct">The cancellation token used to stop batch processing.</param>
    /// <returns>A result containing the successfully completed monitor polling data.</returns>
    /// <exception cref="OperationCanceledException">Batch processing is canceled.</exception>
    public async Task<Result<List<MonitorPollResult>>> ProcessDueMonitorsAsync(CancellationToken ct = default)
    {
        IEnumerable<MonitorPollingRecord> monitors = await _monitorQueries.GetDueEnabledAsync(_options.BatchSize, ct);
        List<MonitorPollResult> monitorsResults = new();

        foreach (MonitorPollingRecord monitor in monitors)
        {
            ct.ThrowIfCancellationRequested();

            Result<MonitorPollResult> monitorsResult = await ProcessMonitorAsync(monitor, ct);
            if (monitorsResult.IsFailed)
            {
                continue;
            }

            monitorsResults.Add(monitorsResult.Value);
        }

        return Result.Ok(monitorsResults);
    }

    /// <summary>
    /// Finds and processes a monitor belonging to the specified organization.
    /// </summary>
    /// <param name="monitorId">The identifier of the monitor to process.</param>
    /// <param name="organizationId">The identifier of the monitor's organization.</param>
    /// <param name="ct">The cancellation token for the polling operation.</param>
    /// <returns>A result containing the completed monitor polling data, or an error if the monitor cannot be processed.</returns>
    /// <exception cref="OperationCanceledException">The polling operation is canceled.</exception>
    public async Task<Result<MonitorPollResult>> ProcessMonitorAsync(Guid monitorId, Guid organizationId, CancellationToken ct)
    {
        MonitorPollingRecord? monitor = await _monitorQueries.GetByIdForPollingAsync(
            monitorId,
            ct);

        if (monitor is null || monitor.OrganizationId != organizationId)
        {
            return Result.Fail(new NotFoundError($"Monitor '{monitorId}' was not found."));
        }

        return await ProcessMonitorAsync(monitor, ct);
    }

    /// <summary>
    /// Processes the supplied monitor polling record.
    /// </summary>
    /// <param name="monitor">The monitor polling record to process.</param>
    /// <param name="ct">The cancellation token for the polling operation.</param>
    /// <returns>A result containing the completed monitor polling data, or an error if polling fails.</returns>
    /// <exception cref="OperationCanceledException">The polling operation is canceled.</exception>
    public async Task<Result<MonitorPollResult>> ProcessMonitorAsync(MonitorPollingRecord monitor, CancellationToken ct)
    {
        try
        {
            CreateMonitorPollResultsInput monitorPollResults = await GetPollResultAsync(monitor, ct);
            MonitorPollResult monitorResult = CreateMonitorPollResult(monitor, monitorPollResults);
            await SavePollResultAsync(monitorResult, monitorPollResults, ct);

            return Result.Ok(monitorResult);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to process monitor. MonitorId: {MonitorId}", monitor.Id);
            return Result.Fail("Failed to process monitor.");
        }
    }

    private async Task<CreateMonitorPollResultsInput> GetPollResultAsync(MonitorPollingRecord monitor, CancellationToken ct)
    {
        HttpMonitorResponse response = await _httpMonitorClient.SendAsync(monitor, ct);
        bool isSuccess = response.IsSuccess;
        string requestStatus = response.RequestStatus;
        string? value = null;

        if (isSuccess)
        {
            bool extractionSucceeded =
                !string.IsNullOrWhiteSpace(response.Body) &&
                _jsonPathReader.TryReadValue(response.Body, monitor.ResultPath, out value) &&
                value is not null;

            if (!extractionSucceeded)
            {
                _logger.LogWarning(
                    "Failed to extract monitor value. MonitorId: {MonitorId}, ResultPath: {ResultPath}",
                    monitor.Id,
                    monitor.ResultPath);

                isSuccess = false;
                requestStatus = RequestStatusNames.ExtractionError;
            }
        }

        return new CreateMonitorPollResultsInput(
            Value: value,
            CheckedAt: DateTime.UtcNow,
            IsSuccess: isSuccess,
            ResponseTimeMs: response.ResponseTimeMs,
            StatusCode: response.StatusCode,
            MonitorId: monitor.Id,
            RequestStatus: requestStatus);
    }

    private async Task SavePollResultAsync(
        MonitorPollResult monitorResult,
        CreateMonitorPollResultsInput resultInput,
        CancellationToken ct)
    {
        await using IUnitOfWork uof = await _unitOfWorkFactory.CreateAsync(ct: ct);
        IDbSession session = (IDbSession)uof;
        UpdateMonitorAfterPollInput updateInput = new(
            monitorResult.MonitorId,
            monitorResult.CurrentValue,
            monitorResult.LastCheckedAt,
            monitorResult.NextExecutionAt,
            monitorResult.Status);

        await _monitorPollResultCommands.CreateAsync(resultInput, session, ct);
        await _monitorCommands.UpdateAfterPollAsync(updateInput, session, ct);

        await uof.CommitAsync(ct);
    }

    private MonitorPollResult CreateMonitorPollResult(MonitorPollingRecord monitor,
        CreateMonitorPollResultsInput resultInput)
    {
        DateTime completedAt = DateTime.UtcNow;
        DateTime nextExecutionAt = completedAt.AddSeconds(monitor.PollingIntervalSeconds);

        string status = resultInput.IsSuccess
            ? nameof(MonitorStatus.Enabled)
            : nameof(MonitorStatus.Error);

        return new MonitorPollResult(monitor.Id, completedAt, nextExecutionAt, status, monitor.OrganizationId)
        {
            CurrentValue = resultInput.Value
        };
    }
}
