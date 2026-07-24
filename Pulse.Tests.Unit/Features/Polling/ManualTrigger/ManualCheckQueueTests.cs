using FluentAssertions;
using Pulse.BL.Features.Polling.ManualTrigger;

namespace Pulse.Tests.Unit.Features.Polling.ManualTrigger;

public class ManualCheckQueueTests
{
    [Fact]
    public void TryEnqueue_WhenQueueHasCapacity_ReturnsTrue()
    {
        // Arrange
        ManualCheckQueue queue = new(capacity: 1);

        // Act
        bool result = queue.TryEnqueue(Guid.NewGuid());

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void TryEnqueue_WhenQueueIsFull_ReturnsFalseWithoutBlocking()
    {
        // Arrange
        ManualCheckQueue queue = new(capacity: 1);
        queue.TryEnqueue(Guid.NewGuid());

        // Act
        bool result = queue.TryEnqueue(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DequeueAsync_ReturnsPreviouslyEnqueuedMonitorId()
    {
        // Arrange
        ManualCheckQueue queue = new(capacity: 10);
        Guid monitorId = Guid.NewGuid();
        queue.TryEnqueue(monitorId);

        // Act
        Guid dequeued = await queue.DequeueAsync(CancellationToken.None);

        // Assert
        dequeued.Should().Be(monitorId);
    }
}
