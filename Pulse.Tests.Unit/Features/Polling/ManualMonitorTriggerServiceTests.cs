using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.BL.Common.BackgroundTasks;
using Pulse.BL.Features.Polling;
using Pulse.BL.Features.Polling.ManualTrigger;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.Tests.Unit.Features.Polling;

public class ManualMonitorTriggerServiceTests
{
    private readonly Mock<IMonitorQueries> _monitorQueries = new();
    private readonly Mock<IPollingService> _pollingService = new();
    private readonly ILogger<ManualMonitorTriggerService> _logger = Mock.Of<ILogger<ManualMonitorTriggerService>>();

    [Fact]
    public async Task TriggerAsync_WhenMonitorDoesNotExist_ReturnsFailureAndDoesNotQueueWorkItem()
    {
        // Arrange
        RecordingBackgroundTaskQueue queue = new();
        ManualMonitorTriggerService service = new(_monitorQueries.Object, queue, _logger);
        Guid monitorId = Guid.NewGuid();

        _monitorQueries
            .Setup(q => q.GetByIdForPollingAsync(monitorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MonitorPollingRecord?)null);

        // Act
        Result result = await service.TriggerAsync(monitorId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Metadata.Should().ContainKey("Code");
        result.Errors[0].Metadata["Code"].Should().Be(ManualMonitorTriggerService.MonitorNotFoundErrorCode);
        queue.EnqueuedWorkItem.Should().BeNull();
    }

    [Fact]
    public async Task TriggerAsync_WhenMonitorExists_QueuesPollingWorkItemAndExecutesIt()
    {
        // Arrange
        RecordingBackgroundTaskQueue queue = new();
        ManualMonitorTriggerService service = new(_monitorQueries.Object, queue, _logger);

        MonitorPollingRecord monitor = new(
            Guid.NewGuid(),
            "https://example.com/health",
            "GET",
            "$.status",
            60,
            10,
            "Enabled");

        _monitorQueries
            .Setup(q => q.GetByIdForPollingAsync(monitor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monitor);

        _pollingService
            .Setup(p => p.ProcessMonitorAsync(monitor, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        ServiceCollection services = new();
        services.AddSingleton<IPollingService>(_pollingService.Object);

        ServiceProvider provider = services.BuildServiceProvider();

        // Act
        Result result = await service.TriggerAsync(monitor.Id, CancellationToken.None);
        Task backgroundTask = queue.EnqueuedWorkItem!(provider, CancellationToken.None);
        await backgroundTask;

        // Assert
        result.IsSuccess.Should().BeTrue();
        queue.EnqueuedWorkItem.Should().NotBeNull();
        _pollingService.Verify(
            p => p.ProcessMonitorAsync(monitor, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private sealed class RecordingBackgroundTaskQueue : IBackgroundTaskQueue
    {
        public Func<IServiceProvider, CancellationToken, Task>? EnqueuedWorkItem { get; private set; }

        public ValueTask EnqueueAsync(Func<IServiceProvider, CancellationToken, Task> workItem)
        {
            EnqueuedWorkItem = workItem;
            return ValueTask.CompletedTask;
        }

        public ValueTask<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken ct)
            => throw new NotSupportedException();
    }
}
