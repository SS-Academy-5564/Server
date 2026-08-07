using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.API.Features.Internal.MonitorNotifications;
using Pulse.BL.Common.Notifications;
using Pulse.BL.Features.Polling;

namespace Pulse.Tests.Unit.Features.Monitors;

public sealed class MonitorNotificationsControllerTests
{
    [Fact]
    public async Task NotifyAsync_WithUpdates_ForwardsBatchAndReturnsNoContent()
    {
        IReadOnlyCollection<MonitorPollResult> updates =
        [
            new (
                Guid.NewGuid(),
                DateTime.UtcNow,
                DateTime.UtcNow.AddMinutes(1),
                "Enabled",
                Guid.NewGuid())
        ];
        Mock<IBatchMonitorNotificationService> notificationService = new();
        MonitorNotificationsController controller = new(notificationService.Object);

        IActionResult result = await controller.NotifyAsync(updates, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        notificationService.Verify(
            service => service.NotifyAsync(updates, CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task NotifyAsync_WithEmptyBatch_ForwardsEmptyBatch()
    {
        Mock<IBatchMonitorNotificationService> notificationService = new();
        MonitorNotificationsController controller = new(notificationService.Object);

        IActionResult result = await controller.NotifyAsync([], CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        notificationService.Verify(
            service => service.NotifyAsync(
                It.Is<IReadOnlyCollection<MonitorPollResult>>(updates => updates.Count == 0),
                CancellationToken.None),
            Times.Once);
    }
}
