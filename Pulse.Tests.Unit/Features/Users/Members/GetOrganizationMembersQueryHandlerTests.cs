using FluentAssertions;
using FluentResults;
using Moq;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Security;
using Pulse.BL.Features.Users.Members;
using Pulse.DAL.Queries.Members;

namespace Pulse.Tests.Unit.Features.Users.Members;

public class GetOrganizationMembersQueryHandlerTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IMemberQueries> _memberQueriesMock;
    private readonly GetOrganizationMembersQueryHandler _sut;

    public GetOrganizationMembersQueryHandlerTests()
    {
        _currentUserServiceMock = new();
        _memberQueriesMock = new();
        _sut = new GetOrganizationMembersQueryHandler(_currentUserServiceMock.Object, _memberQueriesMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenOrganizationIdIsNull_ReturnsUnauthorized()
    {
        _currentUserServiceMock.Setup(x => x.OrganizationId).Returns((Guid?)null);

        Result<OrganizationMembersResult> result = await _sut.HandleAsync(CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.HasError<UnauthorizedError>().Should().BeTrue();
        _memberQueriesMock.Verify(x => x.GetMembersByOrganizationIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenOrganizationHasMembers_ReturnsMemberList()
    {
        Guid organizationId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        var memberRecords = new List<MemberRecord>
        {
            new(userId, "user@example.com", "John", "Doe", "Admin", DateTimeOffset.UtcNow)
        };

        _currentUserServiceMock.Setup(x => x.OrganizationId).Returns(organizationId);
        _memberQueriesMock.Setup(x => x.GetMembersByOrganizationIdAsync(organizationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(memberRecords);

        Result<OrganizationMembersResult> result = await _sut.HandleAsync(CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalCount.Should().Be(1);
        result.Value.Members.Should().ContainSingle();
        result.Value.Members[0].UserId.Should().Be(userId);
        result.Value.Members[0].Email.Should().Be("user@example.com");
        result.Value.Members[0].Name.Should().Be("John Doe");
        result.Value.Members[0].Role.Should().Be("Admin");
        _memberQueriesMock.Verify(x => x.GetMembersByOrganizationIdAsync(organizationId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
