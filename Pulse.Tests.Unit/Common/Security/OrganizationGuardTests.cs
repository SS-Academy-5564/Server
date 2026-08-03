using FluentAssertions;
using FluentResults;
using Moq;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Security;

namespace Pulse.Tests.Unit.Common.Security;

public class OrganizationGuardTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    [Fact]
    public void RequireOrganizationId_WhenUserHasOrganization_ReturnsOrganizationId()
    {
        Guid expectedId = Guid.NewGuid();
        _currentUserServiceMock
            .Setup(x => x.OrganizationId)
            .Returns(expectedId);

        Result<Guid> result = _currentUserServiceMock.Object.RequireOrganizationId();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedId);
    }

    [Fact]
    public void RequireOrganizationId_WhenUserHasNoOrganization_ReturnsUnauthorizedError()
    {
        _currentUserServiceMock
            .Setup(x => x.OrganizationId)
            .Returns((Guid?)null);

        Result<Guid> result = _currentUserServiceMock.Object.RequireOrganizationId();

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is UnauthorizedError);
    }
}
