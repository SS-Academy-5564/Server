using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Pulse.BL.Common.Helpers.Json;
using Pulse.BL.Features.Polling;
using Pulse.BL.Features.Polling.Http;
using Pulse.BL.Features.Polling.Options;
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
    private readonly MonitorRecord _monitor = new(
        Guid.NewGuid(),
        "https://example.com/health",
        "GET",
        "data.status",
        60,
        30);
    private readonly PollingService _service;
    private CreateMonitorPollResultsInput? _createdMonitorPollResults;
    private UpdateMonitorAfterPollInput? _updatedMonitor;

    public PollingServiceTests()
    {
        _monitorPollResultsCommands
            .Setup(c => c.CreateAsync(It.IsAny<CreateMonitorPollResultsInput>(), It.IsAny<IUnitOfWork>(), It.IsAny<CancellationToken>()))
            .Callback<CreateMonitorPollResultsInput, IUnitOfWork, CancellationToken>((input, _, _) => _createdMonitorPollResults = input)
            .Returns(Task.CompletedTask);

        _monitorCommands
            .Setup(c => c.UpdateAfterPollAsync(It.IsAny<UpdateMonitorAfterPollInput>(), It.IsAny<IUnitOfWork>(), It.IsAny<CancellationToken>()))
            .Callback<UpdateMonitorAfterPollInput, IUnitOfWork, CancellationToken>((input, _, _) => _updatedMonitor = input)
            .Returns(Task.CompletedTask);

        _unitOfWork
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWork
            .Setup(u => u.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        _unitOfWorkFactory
            .Setup(f => f.CreateAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWork.Object);

        _service = new PollingService(
            Mock.Of<ILogger<PollingService>>(),
            Options.Create(new PollingWorkerOptions
            {
                BatchSize = 50,
                LoopIntervalSeconds = 10,
                MaxConcurrentRequests = 1
            }),
            _httpMonitorClient.Object,
            _jsonPathReader.Object,
            _monitorQueries.Object,
            _monitorCommands.Object,
            _monitorPollResultsCommands.Object,
            _unitOfWorkFactory.Object);
    }

    [Fact]
    public async Task ProcessDueMonitorsAsync_WhenMonitorSucceeds_PersistsResultAndUpdatesMonitorAsync()
    {
        // Arrange
        HttpMonitorResponse response = new(
            IsSuccess: true,
            ResponseTimeMs: 123,
            RequestStatus: RequestStatusNames.Success)
        {
            Body = """{"data":{"status":"healthy"}}""",
            StatusCode = 200
        };

        _monitorQueries
            .Setup(q => q.GetDueEnabledAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([_monitor]);
        _httpMonitorClient
            .Setup(c => c.SendAsync(_monitor, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        _jsonPathReader
            .Setup(r => r.ReadValue(response.Body, _monitor.ResultPath))
            .Returns("healthy");

        // Act
        await _service.ProcessDueMonitorsAsync();

        // Assert
        _createdMonitorPollResults.Should().NotBeNull();
        _createdMonitorPollResults!.Value.Should().Be("healthy");
        _createdMonitorPollResults.IsSuccess.Should().BeTrue();
        _createdMonitorPollResults.StatusCode.Should().Be(200);
        _createdMonitorPollResults.ResponseTimeMs.Should().Be(123);
        _createdMonitorPollResults.RequestStatus.Should().Be(RequestStatusNames.Success);

        _updatedMonitor.Should().NotBeNull();
        _updatedMonitor!.CurrentValue.Should().Be("healthy");
        _updatedMonitor.NextExecutionAt.Should()
            .Be(_updatedMonitor.LastCheckedAt.AddSeconds(_monitor.PollingIntervalSeconds));

        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessDueMonitorsAsync_WhenExtractionFails_PreservesHttpMetadataAndUsesExtractionErrorAsync()
    {
        // Arrange
        HttpMonitorResponse response = new(
            IsSuccess: true,
            ResponseTimeMs: 321,
            RequestStatus: RequestStatusNames.Success)
        {
            Body = """{"data":{"status":{"nested":"value"}}}""",
            StatusCode = 200
        };

        _monitorQueries
            .Setup(q => q.GetDueEnabledAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([_monitor]);
        _httpMonitorClient
            .Setup(c => c.SendAsync(_monitor, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);
        _jsonPathReader
            .Setup(r => r.ReadValue(response.Body, _monitor.ResultPath))
            .Throws(new JsonException("Invalid path"));

        // Act
        await _service.ProcessDueMonitorsAsync();

        // Assert
        _createdMonitorPollResults.Should().NotBeNull();
        _createdMonitorPollResults!.Value.Should().BeNull();
        _createdMonitorPollResults.IsSuccess.Should().BeFalse();
        _createdMonitorPollResults.StatusCode.Should().Be(200);
        _createdMonitorPollResults.ResponseTimeMs.Should().Be(321);
        _createdMonitorPollResults.RequestStatus.Should().Be(RequestStatusNames.ExtractionError);

        _updatedMonitor.Should().NotBeNull();
        _updatedMonitor!.CurrentValue.Should().BeNull();
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessDueMonitorsAsync_WhenHttpResponseFailed_DoesNotExtractValueAsync()
    {
        // Arrange
        HttpMonitorResponse response = new(
            IsSuccess: false,
            ResponseTimeMs: 222,
            RequestStatus: RequestStatusNames.Failed)
        {
            Body = """{"data":{"status":"unhealthy"}}""",
            StatusCode = 500
        };

        _monitorQueries
            .Setup(q => q.GetDueEnabledAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([_monitor]);
        _httpMonitorClient
            .Setup(c => c.SendAsync(_monitor, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _service.ProcessDueMonitorsAsync();

        // Assert
        _jsonPathReader.Verify(
            r => r.ReadValue(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
        _createdMonitorPollResults.Should().NotBeNull();
        _createdMonitorPollResults!.Value.Should().BeNull();
        _createdMonitorPollResults.IsSuccess.Should().BeFalse();
        _createdMonitorPollResults.StatusCode.Should().Be(500);
        _createdMonitorPollResults.ResponseTimeMs.Should().Be(222);
        _createdMonitorPollResults.RequestStatus.Should().Be(RequestStatusNames.Failed);

        _updatedMonitor.Should().NotBeNull();
        _updatedMonitor!.CurrentValue.Should().BeNull();
    }

    [Fact]
    public async Task ProcessDueMonitorsAsync_WhenHttpClientThrowsUnexpectedException_PersistsUnexpectedErrorAsync()
    {
        // Arrange
        _monitorQueries
            .Setup(q => q.GetDueEnabledAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([_monitor]);
        _httpMonitorClient
            .Setup(c => c.SendAsync(_monitor, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected failure"));

        // Act
        await _service.ProcessDueMonitorsAsync();

        // Assert
        _createdMonitorPollResults.Should().NotBeNull();
        _createdMonitorPollResults!.Value.Should().BeNull();
        _createdMonitorPollResults.IsSuccess.Should().BeFalse();
        _createdMonitorPollResults.StatusCode.Should().BeNull();
        _createdMonitorPollResults.ResponseTimeMs.Should().Be(0);
        _createdMonitorPollResults.RequestStatus.Should().Be(RequestStatusNames.UnexpectedError);
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessDueMonitorsAsync_WhenNoMonitorsAreDue_DoesNotProcessAnythingAsync()
    {
        // Arrange
        _monitorQueries
            .Setup(q => q.GetDueEnabledAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _service.ProcessDueMonitorsAsync();

        // Assert
        _httpMonitorClient.Verify(
            c => c.SendAsync(It.IsAny<MonitorRecord>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _monitorPollResultsCommands.Verify(
            c => c.CreateAsync(It.IsAny<CreateMonitorPollResultsInput>(), It.IsAny<IUnitOfWork>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessDueMonitorsAsync_WhenCancellationIsRequested_ThrowsOperationCanceledExceptionAsync()
    {
        // Arrange
        _monitorQueries
            .Setup(q => q.GetDueEnabledAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([_monitor]);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        Func<Task> act = () => _service.ProcessDueMonitorsAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
