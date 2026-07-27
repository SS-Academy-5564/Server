using FluentAssertions;
using FluentResults;
using Moq;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Pagination;
using Pulse.BL.Common.Security;
using Pulse.BL.Features.Users.Members;
using Pulse.DAL.Common.Pagination;
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

        Result<PagedResult<OrganizationMemberResult>> result = await _sut.HandleAsync(
            new GetOrganizationMembersQuery(null, null),
            CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.HasError<UnauthorizedError>().Should().BeTrue();
        _memberQueriesMock.Verify(
            x => x.GetMembersByOrganizationIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenPaginationIsMissing_UsesDefaultsAndReturnsMemberPage()
    {
        Guid organizationId = Guid.NewGuid();
        Guid userId = Guid.NewGuid();
        var memberRecords = new List<MemberRecord>
        {
            new(userId, "user@example.com", "John", "Doe", "Admin", DateTimeOffset.UtcNow)
        };

        _currentUserServiceMock.Setup(x => x.OrganizationId).Returns(organizationId);
        _memberQueriesMock
            .Setup(x => x.GetMembersByOrganizationIdAsync(
                organizationId,
                PaginationDefaults.PageNumber,
                PaginationDefaults.PageSize,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedRecords<MemberRecord>(memberRecords, 21));

        Result<PagedResult<OrganizationMemberResult>> result = await _sut.HandleAsync(
            new GetOrganizationMembersQuery(null, null),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PageNumber.Should().Be(PaginationDefaults.PageNumber);
        result.Value.PageSize.Should().Be(PaginationDefaults.PageSize);
        result.Value.TotalCount.Should().Be(21);
        result.Value.TotalPages.Should().Be(3);
        result.Value.Items.Should().ContainSingle();
        result.Value.Items[0].UserId.Should().Be(userId);
        result.Value.Items[0].Email.Should().Be("user@example.com");
        result.Value.Items[0].Name.Should().Be("John Doe");
        result.Value.Items[0].Role.Should().Be("Admin");
    }

    [Fact]
    public async Task HandleAsync_WithCustomPagination_PassesPaginationToQueries()
    {
        Guid organizationId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.OrganizationId).Returns(organizationId);
        _memberQueriesMock
            .Setup(x => x.GetMembersByOrganizationIdAsync(
                organizationId,
                2,
                25,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedRecords<MemberRecord>([], 30));

        Result<PagedResult<OrganizationMemberResult>> result = await _sut.HandleAsync(
            new GetOrganizationMembersQuery(2, 25),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.PageNumber.Should().Be(2);
        result.Value.PageSize.Should().Be(25);
        result.Value.TotalCount.Should().Be(30);
        result.Value.TotalPages.Should().Be(2);
        _memberQueriesMock.Verify(
            x => x.GetMembersByOrganizationIdAsync(
                organizationId,
                2,
                25,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
