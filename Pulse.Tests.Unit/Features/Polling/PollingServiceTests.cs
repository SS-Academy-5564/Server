using System.Data;
using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Helpers.Json;
using Pulse.BL.Features.Polling;
using Pulse.BL.Features.Polling.Http;
using Pulse.DAL.Commands.MonitorPollResults;
using Pulse.DAL.Commands.Monitors;
using Pulse.DAL.Common.Constants;
using Pulse.DAL.Common.Repository;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.Tests.Unit.Features.Polling;

public class PollingServiceTests
{
    private readonly Mock<IHttpMonitorClient> _httpMonitorClient = new();
    private readonly Mock<IJsonPathReader> _jsonPathReader = new();
    private readonly Mock<IMonitorCommands> _monitorCommands = new();
    private readonly Mock<IMonitorPollResultsCommands> _monitorPollResultsCommands = new();
    private readonly Mock<IMonitorQueries> _monitorQueries = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new();
    private readonly MonitorPollingRecord _monitor = new(
        Guid.NewGuid(),
        "https://example.com/health",
        "GET",
        "data.status",
        60,
        30,
        "Enabled",
        Guid.NewGuid());
    private readonly PollingService _service;
    private CreateMonitorPollResultsInput? _createdMonitorPollResults;
    private UpdateMonitorAfterPollInput? _updatedMonitor;
    private IDbSession? _createdMonitorPollResultsSession;
    private IDbSession? _updatedMonitorSession;

    public PollingServiceTests()
    {
        _unitOfWork.As<IDbSession>();

        _monitorPollResultsCommands
            .Setup(c => c.CreateAsync(
                It.IsAny<CreateMonitorPollResultsInput>(),
                It.IsAny<IDbSession>(),
                It.IsAny<CancellationToken>()))
            .Callback<CreateMonitorPollResultsInput, IDbSession, CancellationToken>((input, session, _) =>
            {
                _createdMonitorPollResults = input;
                _createdMonitorPollResultsSession = session;
            })
            .Returns(Task.CompletedTask);

        _monitorCommands
            .Setup(c => c.UpdateAfterPollAsync(
                It.IsAny<UpdateMonitorAfterPollInput>(),
                It.IsAny<IDbSession>(),
                It.IsAny<CancellationToken>()))
            .Callback<UpdateMonitorAfterPollInput, IDbSession, CancellationToken>((input, session, _) =>
            {
                _updatedMonitor = input;
                _updatedMonitorSession = session;
            })
            .Returns(Task.CompletedTask);

        _unitOfWork
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWork
            .Setup(u => u.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        _unitOfWorkFactory
            .Setup(f => f.CreateAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWork.Object);

        _service = new PollingService(
            Mock.Of<ILogger<PollingService>>(),
            _httpMonitorClient.Object,
            _jsonPathReader.Object,
            _monitorQueries.Object,
            _monitorCommands.Object,
            _monitorPollResultsCommands.Object,
            _unitOfWorkFactory.Object);
    }

    private void SetupHttpResponse(MonitorPollingRecord monitor, HttpMonitorResponse response)
        => _httpMonitorClient
            .Setup(c => c.SendAsync(monitor, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

    private void SetupJsonExtraction(string json, string path, bool succeeds, string? extractedValue)
    {
        string? value = extractedValue;

        _jsonPathReader
            .Setup(r => r.TryReadValue(json, path, out value))
            .Returns(succeeds);
    }

    private void AssertSavedPollResult(string? value, bool isSuccess, int? statusCode, int responseTimeMs, string requestStatus)
    {
        _createdMonitorPollResults.Should().NotBeNull();
        _createdMonitorPollResults!.Value.Should().Be(value);
        _createdMonitorPollResults.IsSuccess.Should().Be(isSuccess);
        _createdMonitorPollResults.StatusCode.Should().Be(statusCode);
        _createdMonitorPollResults.ResponseTimeMs.Should().Be(responseTimeMs);
        _createdMonitorPollResults.RequestStatus.Should().Be(requestStatus);
    }

    private void AssertMonitorUpdateCommandReceived(string? currentValue)
    {
        _updatedMonitor.Should().NotBeNull();
        _updatedMonitor!.CurrentValue.Should().Be(currentValue);
    }

    [Fact]
    public async Task GetDueEnabledAsync_WhenMonitorsAreDue_ReturnsMonitorsAsync()
    {
        // Arrange
        const int numberOfRecords = 50;
        _monitorQueries
            .Setup(q => q.GetDueEnabledAsync(numberOfRecords, It.IsAny<CancellationToken>()))
            .ReturnsAsync([_monitor]);

        // Act
        Result<IEnumerable<MonitorPollingRecord>> result = await _service.GetDueEnabledAsync(numberOfRecords, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle().Which.Should().Be(_monitor);
    }

    [Fact]
    public async Task GetDueEnabledAsync_WhenNoMonitorsAreDue_ReturnsEmptyEnumerableAsync()
    {
        // Arrange
        const int numberOfRecords = 50;
        _monitorQueries
            .Setup(q => q.GetDueEnabledAsync(numberOfRecords, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        Result<IEnumerable<MonitorPollingRecord>> result = await _service.GetDueEnabledAsync(numberOfRecords, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task ProcessMonitorAsync_WhenMonitorIdDoesNotExist_ReturnsNotFoundError()
    {
        // Arrange
        Guid monitorId = Guid.NewGuid();
        Guid organizationId = Guid.NewGuid();
        _monitorQueries
            .Setup(q => q.GetByIdForPollingAsync(monitorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MonitorPollingRecord?)null);

        // Act
        Result<MonitorPollResult> result = await _service.ProcessMonitorAsync(
            monitorId,
            organizationId,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().BeOfType<NotFoundError>();
    }

    [Fact]
    public async Task ProcessMonitorAsync_WhenMonitorIdExists_DelegatesToRecordOverload()
    {
        // Arrange
        Guid monitorId = Guid.NewGuid();
        Guid organizationId = Guid.NewGuid();
        MonitorPollingRecord monitor = _monitor with { Id = monitorId };
        HttpMonitorResponse response = new(
            IsSuccess: true,
            ResponseTimeMs: 123,
            RequestStatus: RequestStatusNames.Success)
        {
            Body = "{\"data\":{\"status\":\"healthy\"}}",
            StatusCode = 200
        };

        _monitorQueries
            .Setup(q => q.GetByIdForPollingAsync(monitorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monitor);

        _httpMonitorClient
            .Setup(c => c.SendAsync(monitor, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        _jsonPathReader
            .Setup(r => r.TryReadValue(response.Body, monitor.ResultPath, out It.Ref<string?>.IsAny))
            .Returns(true);

        // Act
        Result<MonitorPollResult> result = await _service.ProcessMonitorAsync(
            monitorId,
            organizationId,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _monitorQueries.Verify(
            q => q.GetByIdForPollingAsync(monitorId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessMonitorAsync_WhenMonitorSucceeds_PersistsResultAndUpdatesMonitorAsync()
    {
        // Arrange
        HttpMonitorResponse response = new(
            IsSuccess: true,
            ResponseTimeMs: 123,
            RequestStatus: RequestStatusNames.Success)
        {
            Body = """
                   {
                    "data":
                        {
                            "status":"healthy"
                        }
                    }
                   """,
            StatusCode = 200
        };

        SetupHttpResponse(_monitor, response);
        SetupJsonExtraction(response.Body, _monitor.ResultPath, succeeds: true, extractedValue: "healthy");

        // Act
        Result<MonitorPollResult> result = await _service.ProcessMonitorAsync(_monitor, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        AssertSavedPollResult("healthy", isSuccess: true, statusCode: 200, responseTimeMs: 123, RequestStatusNames.Success);

        AssertMonitorUpdateCommandReceived("healthy");

        _updatedMonitor!.NextExecutionAt.Should()
            .Be(_updatedMonitor.LastCheckedAt.AddSeconds(_monitor.PollingIntervalSeconds));
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _createdMonitorPollResultsSession.Should().BeSameAs(_unitOfWork.Object);
        _updatedMonitorSession.Should().BeSameAs(_unitOfWork.Object);
    }

    [Fact]
    public async Task ProcessMonitorAsync_WhenHttpResponseFailed_DoesNotExtractValueAsync()
    {
        // Arrange
        HttpMonitorResponse response = new(
            IsSuccess: false,
            ResponseTimeMs: 222,
            RequestStatus: RequestStatusNames.Failed)
        {
            Body = """
                   {
                    "data":
                        {"status":"unhealthy"}
                   }
                   """,
            StatusCode = 500
        };

        SetupHttpResponse(_monitor, response);

        // Act
        Result<MonitorPollResult> result = await _service.ProcessMonitorAsync(_monitor, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _jsonPathReader.Verify(
            r => r.TryReadValue(It.IsAny<string>(), It.IsAny<string>(), out It.Ref<string?>.IsAny),
            Times.Never);

        AssertSavedPollResult(null, isSuccess: false, statusCode: 500, responseTimeMs: 222, RequestStatusNames.Failed);
        AssertMonitorUpdateCommandReceived(null);
    }

    [Fact]
    public async Task ProcessMonitorAsync_WhenValueExtractionFails_PersistsExtractionErrorAndUpdatesMonitorAsync()
    {
        // Arrange
        HttpMonitorResponse response = new(
            IsSuccess: true,
            ResponseTimeMs: 123,
            RequestStatus: RequestStatusNames.Success)
        {
            Body = """{"data":"not-object"}""",
            StatusCode = 200
        };

        SetupHttpResponse(_monitor, response);
        SetupJsonExtraction(response.Body, _monitor.ResultPath, succeeds: false, extractedValue: null);

        // Act
        Result<MonitorPollResult> result = await _service.ProcessMonitorAsync(_monitor, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        AssertSavedPollResult(null, isSuccess: false, statusCode: 200, responseTimeMs: 123, RequestStatusNames.ExtractionError);
        AssertMonitorUpdateCommandReceived(null);

        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessMonitorAsync_WhenExpectedValueIsMissing_PersistsExtractionErrorAndUpdatesMonitorAsync()
    {
        // Arrange
        HttpMonitorResponse response = new(
            IsSuccess: true,
            ResponseTimeMs: 123,
            RequestStatus: RequestStatusNames.Success)
        {
            Body = """{"status":"ok"}""",
            StatusCode = 200
        };

        SetupHttpResponse(_monitor, response);
        SetupJsonExtraction(response.Body, _monitor.ResultPath, succeeds: true, extractedValue: null);

        // Act
        Result<MonitorPollResult> result = await _service.ProcessMonitorAsync(_monitor, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        AssertSavedPollResult(null, isSuccess: false, statusCode: 200, responseTimeMs: 123, RequestStatusNames.ExtractionError);
        AssertMonitorUpdateCommandReceived(null);

        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessMonitorAsync_WhenSuccessfulResponseBodyIsEmpty_PersistsExtractionErrorAndUpdatesMonitorAsync()
    {
        // Arrange
        HttpMonitorResponse response = new(
            IsSuccess: true,
            ResponseTimeMs: 123,
            RequestStatus: RequestStatusNames.Success)
        {
            Body = "",
            StatusCode = 200
        };

        SetupHttpResponse(_monitor, response);

        // Act
        Result<MonitorPollResult> result = await _service.ProcessMonitorAsync(_monitor, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _jsonPathReader.Verify(
            r => r.TryReadValue(It.IsAny<string>(), It.IsAny<string>(), out It.Ref<string?>.IsAny),
            Times.Never);

        AssertSavedPollResult(null, isSuccess: false, statusCode: 200, responseTimeMs: 123, RequestStatusNames.ExtractionError);

        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessMonitorAsync_WhenExceptionThrown_ReturnsFailedResultAsync()
    {
        // Arrange
        _httpMonitorClient
            .Setup(c => c.SendAsync(_monitor, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Polling failed."));

        // Act
        Result<MonitorPollResult> result = await _service.ProcessMonitorAsync(_monitor, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Message.Should().Be("Failed to process monitor.");
        _unitOfWorkFactory.Verify(
            f => f.CreateAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessMonitorAsync_WhenCancellationIsRequested_ThrowsOperationCanceledExceptionAsync()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        _httpMonitorClient
            .Setup(c => c.SendAsync(_monitor, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        // Act
        Func<Task> act = () => _service.ProcessMonitorAsync(_monitor, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task ProcessMonitorAsync_WhenPollingFails_SetsMonitorStatusToErrorAsync()
    {
        // Arrange
        HttpMonitorResponse response = new(
            IsSuccess: false,
            ResponseTimeMs: 100,
            RequestStatus: RequestStatusNames.Failed)
        {
            Body = null,
            StatusCode = 500
        };

        SetupHttpResponse(_monitor, response);

        // Act
        Result<MonitorPollResult> result = await _service.ProcessMonitorAsync(_monitor, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _updatedMonitor.Should().NotBeNull();
        _updatedMonitor!.Status.Should().Be("Error");
    }
}
