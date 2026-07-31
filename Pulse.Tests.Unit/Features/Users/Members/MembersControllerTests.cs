using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.API.Features.Users.Members;
using Pulse.API.Responses;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Pagination;
using Pulse.BL.Features.Users.Members;

namespace Pulse.Tests.Unit.Features.Users.Members;

/// <summary>
/// Contains unit tests for the <see cref="MembersController"/>.
/// </summary>
public class MembersControllerTests
{
    private readonly Mock<IAsyncHandler<
        GetOrganizationMembersQuery,
        Result<PagedResult<OrganizationMemberResult>>>> _handlerMock;
    private readonly MembersController _sut;

    /// <summary>
    /// Initializes a new instance of the <see cref="MembersControllerTests"/> class.
    /// </summary>
    public MembersControllerTests()
    {
        _handlerMock = new();
        _sut = new MembersController(_handlerMock.Object);
    }

    /// <summary>
    /// Tests that the endpoint returns a paged response when successful.
    /// </summary>
    [Fact]
    public async Task GetOrganizationMembersAsync_WhenSuccessful_ReturnsPagedResponse()
    {
        IReadOnlyList<OrganizationMemberResult> members = new List<OrganizationMemberResult>
        {
            new(
                Guid.NewGuid(),
                "John Doe",
                "user@example.com",
                "Admin",
                DateTimeOffset.UtcNow)
        }.AsReadOnly();

        _handlerMock
            .Setup(x => x.HandleAsync(
                It.Is<GetOrganizationMembersQuery>(q => q.PageNumber == 2 && q.PageSize == 25),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new PagedResult<OrganizationMemberResult>(members, 2, 25, 51)));

        IActionResult result = await _sut.GetOrganizationMembersAsync(
            new GetOrganizationMembersRequest(2, 25),
            CancellationToken.None);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ApiResponse<IReadOnlyList<OrganizationMemberResult>> response = ok.Value
            .Should()
            .BeOfType<ApiResponse<IReadOnlyList<OrganizationMemberResult>>>()
            .Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(members);
        response.Pagination.Should().NotBeNull();
        response.Pagination!.PageNumber.Should().Be(2);
        response.Pagination.PageSize.Should().Be(25);
        response.Pagination.TotalCount.Should().Be(51);
        response.Pagination.TotalPages.Should().Be(3);
        _handlerMock.Verify(
            x => x.HandleAsync(
                It.Is<GetOrganizationMembersQuery>(q => q.PageNumber == 2 && q.PageSize == 25),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
