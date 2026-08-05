using FluentAssertions;
using Microsoft.Extensions.Options;
using Pulse.BL.Features.Polling.ManualCheck.Queue;

namespace Pulse.Tests.Unit.Features.Polling.ManualCheck;

public class ManualCheckQueueTests
{
    private static ManualCheckQueue CreateQueue(int capacity)
        => new(Options.Create(new ManualCheckQueueOptions { Capacity = capacity }));

    [Fact]
    public void TryEnqueue_WhenQueueHasCapacity_ReturnsTrue()
    {
        // Arrange
        ManualCheckQueue queue = CreateQueue(capacity: 1);
        ManualCheckJob job = new(Guid.NewGuid(), Guid.NewGuid());

        // Act
        bool result = queue.TryEnqueue(job);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void TryEnqueue_WhenQueueIsFull_ReturnsFalseWithoutBlocking()
    {
        // Arrange
        ManualCheckQueue queue = CreateQueue(capacity: 1);
        queue.TryEnqueue(new ManualCheckJob(Guid.NewGuid(), Guid.NewGuid()));

        // Act
        bool result = queue.TryEnqueue(new ManualCheckJob(Guid.NewGuid(), Guid.NewGuid()));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DequeueAsync_ReturnsPreviouslyEnqueuedJob()
    {
        // Arrange
        ManualCheckQueue queue = CreateQueue(capacity: 10);
        ManualCheckJob job = new(Guid.NewGuid(), Guid.NewGuid());
        queue.TryEnqueue(job);

        // Act
        ManualCheckJob dequeued = await queue.DequeueAsync(CancellationToken.None);

        // Assert
        dequeued.Should().Be(job);
    }
}
