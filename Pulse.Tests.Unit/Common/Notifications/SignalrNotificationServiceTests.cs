using Microsoft.AspNetCore.SignalR;
using Moq;
using Pulse.API.Common.Notifications;
using Pulse.API.Hubs;
using Pulse.BL.Features.Polling;

namespace Pulse.Tests.Unit.Common.Notifications;

public sealed class SignalrNotificationServiceTests
{
    [Fact]
    public async Task NotifyAsync_ValidOrganizationId_SendsUpdateToSpecifiedOrganizationGroup()
    {
        // Arrange
        Guid organizationId = Guid.NewGuid();
        List<MonitorPollResult> update = new(){new MonitorPollResult(
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(1),
            "Enabled",
            organizationId)
        {
            CurrentValue = "healthy"
        }};

        Mock<INotificationClient> client = new();
        Mock<IHubClients<INotificationClient>> clients = new();
        clients
            .Setup(c => c.Group(organizationId.ToString()))
            .Returns(client.Object);

        Mock<IHubContext<PulseNotificationHub, INotificationClient>> hubContext = new();
        hubContext.SetupGet(h => h.Clients).Returns(clients.Object);

        SignalrNotificationService service = new(hubContext.Object);
        using var cts = new CancellationTokenSource();

        // Act
        await service.NotifyAsync(update, cts.Token);

        // Assert
        client.Verify(c => c.SendUpdatedMonitorsAsync(It.Is<List<MonitorPollResult>>(l => l.SequenceEqual(update)), cts.Token), Times.Once);
    }

    [Fact]
    public async Task NotifyAsync_MultipleOrganizations_SendsUpdatesOnlyToRespectiveOrganizationGroups()
    {
        // Arrange
        Guid org1 = Guid.NewGuid();
        Guid org2 = Guid.NewGuid();

        MonitorPollResult monitor1 = new(
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(1),
            "Enabled",
            org1)
        { CurrentValue = "healthy1" };

        MonitorPollResult monitor2 = new(
            Guid.NewGuid(),
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(1),
            "Enabled",
            org2)
        { CurrentValue = "healthy2" };

        List<MonitorPollResult> batch = [monitor1, monitor2];

        Mock<INotificationClient> client1 = new();
        Mock<INotificationClient> client2 = new();
        Mock<IHubClients<INotificationClient>> clients = new();

        clients.Setup(c => c.Group(org1.ToString())).Returns(client1.Object);
        clients.Setup(c => c.Group(org2.ToString())).Returns(client2.Object);

        Mock<IHubContext<PulseNotificationHub, INotificationClient>> hubContext = new();
        hubContext.SetupGet(h => h.Clients).Returns(clients.Object);

        SignalrNotificationService service = new(hubContext.Object);
        using var cts = new CancellationTokenSource();

        // Act
        await service.NotifyAsync(batch, cts.Token);

        // Assert
        client1.Verify(
            c => c.SendUpdatedMonitorsAsync(
                It.Is<List<MonitorPollResult>>(list => list.Count == 1 && list[0].OrganizationId == org1),
                cts.Token),
            Times.Once);

        client2.Verify(
            c => c.SendUpdatedMonitorsAsync(
                It.Is<List<MonitorPollResult>>(list => list.Count == 1 && list[0].OrganizationId == org2),
                cts.Token),
            Times.Once);
    }
}
