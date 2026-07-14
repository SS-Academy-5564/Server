using FluentAssertions;
using FluentResults;
using Moq;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Security;
using Pulse.BL.Features.Users.Me;
using Pulse.DAL.Queries.Members;

namespace Pulse.Tests.Unit.Features.Users.Me;

public class GetCurrentUserOrganizationsQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IMemberQueries> _memberQueriesMock;
    private readonly GetCurrentUserOrganizationsQueryHandler _sut;

    public GetCurrentUserOrganizationsQueryHandlerTests()
    {
        _currentUserServiceMock = new();
        _memberQueriesMock = new();
        _sut = new GetCurrentUserOrganizationsQueryHandler(_currentUserServiceMock.Object, _memberQueriesMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenUserIdIsNull_ReturnsUnauthorized()
    {
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);

        Result<IReadOnlyList<UserOrganizationResult>> result = await _sut.HandleAsync(CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.HasError<UnauthorizedError>().Should().BeTrue();
        _memberQueriesMock.Verify(
            x => x.GetOrganizationsByUserIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenUserHasMemberships_ReturnsMappedList()
    {
        Guid userId = Guid.NewGuid();
        var record = new UserOrganizationRecord(
            Guid.NewGuid(), "Acme", Guid.NewGuid(), "Admin", DateTimeOffset.UtcNow);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _memberQueriesMock.Setup(x => x.GetOrganizationsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { record });

        Result<IReadOnlyList<UserOrganizationResult>> result = await _sut.HandleAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();
        UserOrganizationResult organization = result.Value[0];
        organization.OrganizationId.Should().Be(record.OrganizationId);
        organization.OrganizationName.Should().Be(record.OrganizationName);
        organization.RoleId.Should().Be(record.RoleId);
        organization.RoleName.Should().Be(record.RoleName);
        organization.JoinedAt.Should().Be(record.JoinedAt);
    }

    [Fact]
    public async Task HandleAsync_WhenUserHasNoMemberships_ReturnsEmptyList()
    {
        Guid userId = Guid.NewGuid();

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _memberQueriesMock.Setup(x => x.GetOrganizationsByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<UserOrganizationRecord>());

        Result<IReadOnlyList<UserOrganizationResult>> result = await _sut.HandleAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
