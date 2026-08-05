using Microsoft.AspNetCore.SignalR;
using Moq;
using Pulse.API.Common.Notifications;
using Pulse.API.Hubs;
using Pulse.DAL.Commands.Monitors;

namespace Pulse.Tests.Unit.Common.Notifications;

public sealed class SignalrNotificationServiceTests
{
    [Fact]
    public async Task NotifyAsync_SendsUpdateToSpecifiedOrganizationGroup()
    {
        // Arrange
        Guid organizationId = Guid.NewGuid();
        UpdateMonitorAfterPollInput update = new(
            Guid.NewGuid(),
            "healthy",
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(1),
            "Enabled");

        Mock<INotificationClient> client = new();
        Mock<IHubClients<INotificationClient>> clients = new();
        clients
            .Setup(c => c.Group(organizationId.ToString()))
            .Returns(client.Object);

        Mock<IHubContext<PulseNotificationHub, INotificationClient>> hubContext = new();
        hubContext.SetupGet(h => h.Clients).Returns(clients.Object);

        SignalrNotificationService service = new(hubContext.Object);

        // Act
        await service.NotifyAsync(organizationId, update, CancellationToken.None);

        // Assert
        client.Verify(c => c.SendUpdatedMonitorAsync(update), Times.Once);
    }
}
