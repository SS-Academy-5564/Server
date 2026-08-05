using Microsoft.AspNetCore.SignalR;
using Moq;
using Pulse.API.Common.Notifications;
using Pulse.API.Hubs;
using Pulse.BL.Features.Polling;

namespace Pulse.Tests.Unit.Common.Notifications;

public sealed class SignalrNotificationServiceTests
{
    [Fact]
    public async Task NotifyAsync_SendsUpdateToSpecifiedOrganizationGroup()
    {
        // Arrange
        Guid organizationId = Guid.NewGuid();
        MonitorPollResult update = new(
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(1),
            "Enabled",
            organizationId)
        {
            CurrentValue = "healthy"
        };

        Mock<INotificationClient> client = new();
        Mock<IHubClients<INotificationClient>> clients = new();
        clients
            .Setup(c => c.Group(organizationId.ToString()))
            .Returns(client.Object);

        Mock<IHubContext<PulseNotificationHub, INotificationClient>> hubContext = new();
        hubContext.SetupGet(h => h.Clients).Returns(clients.Object);

        SignalrNotificationService service = new(hubContext.Object);

        // Act
        await service.NotifyAsync(update, CancellationToken.None);

        // Assert
        client.Verify(c => c.SendUpdatedMonitorAsync(update), Times.Once);
    }
}
