using FluentResults;
using Pulse.BL.Features.DefaultOrganization;
using Pulse.DAL.Common.Constants;

namespace Pulse.Tests.Unit.Features.DefaultOrganization;

public class GetDefaultOrganizationHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsDefaultOrganizationId()
    {
        // Arrange
        GetDefaultOrganizationHandler handler = new();
        GetDefaultOrganizationQuery query = new();

        // Act
        Result<GetDefaultOrganizationResult> result = await handler.HandleAsync(
            query,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(
            SeededIds.Organizations.Default,
            result.Value.DefaultOrganizationId);
    }
}
