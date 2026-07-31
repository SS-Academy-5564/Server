using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.BL.Common.Errors;
using Pulse.BL.Features.Polling.ManualCheck;
using Pulse.BL.Features.Polling.ManualCheck.Queue;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.Tests.Unit.Features.Polling.ManualCheck;

public class ManualCheckHandlerTests
{
    private readonly Mock<IMonitorQueries> _monitorQueries = new();
    private readonly Mock<IManualCheckQueue> _queue = new();
    private readonly ILogger<ManualCheckHandler> _logger = Mock.Of<ILogger<ManualCheckHandler>>();

    private ManualCheckHandler CreateHandler()
        => new(_monitorQueries.Object, _queue.Object, _logger);

    [Fact]
    public async Task HandleAsync_WhenMonitorDoesNotExist_ReturnsNotFoundAndDoesNotEnqueue()
    {
        // Arrange
        ManualCheckHandler handler = CreateHandler();
        Guid monitorId = Guid.NewGuid();

        _monitorQueries
            .Setup(q => q.GetByIdForPollingAsync(monitorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MonitorPollingRecord?)null);

        // Act
        Result result = await handler.HandleAsync(new ManualCheckCommand(monitorId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().BeOfType<NotFoundError>();

        _queue.Verify(q => q.TryEnqueue(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenMonitorExistsAndQueueHasCapacity_EnqueuesMonitorIdAndReturnsSuccess()
    {
        // Arrange
        ManualCheckHandler handler = CreateHandler();

        MonitorPollingRecord monitor = new(
            Guid.NewGuid(),
            "https://example.com/health",
            "GET",
            "status",
            60,
            10,
            "Enabled");

        _monitorQueries
            .Setup(q => q.GetByIdForPollingAsync(monitor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monitor);

        _queue
            .Setup(q => q.TryEnqueue(monitor.Id))
            .Returns(true);

        // Act
        Result result = await handler.HandleAsync(new ManualCheckCommand(monitor.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _queue.Verify(q => q.TryEnqueue(monitor.Id), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenQueueIsFull_ReturnsTooManyRequestsError()
    {
        // Arrange
        ManualCheckHandler handler = CreateHandler();

        MonitorPollingRecord monitor = new(
            Guid.NewGuid(),
            "https://example.com/health",
            "GET",
            "status",
            60,
            10,
            "Enabled");

        _monitorQueries
            .Setup(q => q.GetByIdForPollingAsync(monitor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monitor);

        _queue
            .Setup(q => q.TryEnqueue(monitor.Id))
            .Returns(false);

        // Act
        Result result = await handler.HandleAsync(new ManualCheckCommand(monitor.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().BeOfType<TooManyRequestsError>();
    }
}
