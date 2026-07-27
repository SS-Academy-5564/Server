using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.API.Features.Users.Me;
using Pulse.API.Responses;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.Users.Me;

namespace Pulse.Tests.Unit.Features.Users.Me;

public class CurrentUserControllerTests
{
    private readonly Mock<IAsyncQueryHandler<Result<UserProfileResult>>> _queryMock;
    private readonly Mock<IAsyncQueryHandler<Result<IReadOnlyList<UserOrganizationResult>>>> _organizationsQueryMock;
    private readonly CurrentUserController _sut;

    public CurrentUserControllerTests()
    {
        _queryMock = new();
        _organizationsQueryMock = new();
        _sut = new CurrentUserController(_queryMock.Object, _organizationsQueryMock.Object);
    }

    [Fact]
    public async Task GetCurrentUser_WhenSuccess_Returns200()
    {
        var profile = new UserProfileResult(Guid.NewGuid(), "user@example.com", "John", "Doe", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        _queryMock.Setup(x => x.HandleAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(profile));

        IActionResult result = await _sut.GetCurrentUserAsync(CancellationToken.None);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);

        ApiResponse<UserProfileResult> response = ok.Value.Should().BeOfType<ApiResponse<UserProfileResult>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(profile);
    }

    [Fact]
    public async Task GetCurrentUser_WhenUnauthorized_Returns401()
    {
        _queryMock.Setup(x => x.HandleAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(new UnauthorizedError("User identity not found.")));

        IActionResult result = await _sut.GetCurrentUserAsync(CancellationToken.None);

        ObjectResult obj = result.Should().BeAssignableTo<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(401);

        ApiResponse response = obj.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Success.Should().BeFalse();
        response.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetCurrentUser_WhenNotFound_Returns404()
    {
        _queryMock.Setup(x => x.HandleAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(new NotFoundError("User not found.")));

        IActionResult result = await _sut.GetCurrentUserAsync(CancellationToken.None);

        ObjectResult obj = result.Should().BeAssignableTo<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(404);

        ApiResponse response = obj.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Success.Should().BeFalse();
        response.Errors[0].Code.Should().Be(AppError.Codes.NotFound);
    }

    [Fact]
    public async Task GetCurrentUserOrganizations_WhenSuccess_Returns200()
    {
        var organizations = new List<UserOrganizationResult>
        {
            new(Guid.NewGuid(), "Acme", Guid.NewGuid(), "Admin", DateTimeOffset.UtcNow)
        };

        _organizationsQueryMock.Setup(x => x.HandleAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyList<UserOrganizationResult>>(organizations));

        IActionResult result = await _sut.GetCurrentUserOrganizationsAsync(CancellationToken.None);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);

        ApiResponse<IReadOnlyList<UserOrganizationResult>> response =
            ok.Value.Should().BeOfType<ApiResponse<IReadOnlyList<UserOrganizationResult>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(organizations);
    }

    [Fact]
    public async Task GetCurrentUserOrganizations_WhenEmpty_Returns200WithEmptyArray()
    {
        _organizationsQueryMock.Setup(x => x.HandleAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok<IReadOnlyList<UserOrganizationResult>>(new List<UserOrganizationResult>()));

        IActionResult result = await _sut.GetCurrentUserOrganizationsAsync(CancellationToken.None);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);

        ApiResponse<IReadOnlyList<UserOrganizationResult>> response =
            ok.Value.Should().BeOfType<ApiResponse<IReadOnlyList<UserOrganizationResult>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetCurrentUserOrganizations_WhenUnauthorized_Returns401()
    {
        _organizationsQueryMock.Setup(x => x.HandleAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(new UnauthorizedError("User identity not found.")));

        IActionResult result = await _sut.GetCurrentUserOrganizationsAsync(CancellationToken.None);

        ObjectResult obj = result.Should().BeAssignableTo<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(401);

        ApiResponse response = obj.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Success.Should().BeFalse();
        response.Errors.Should().NotBeEmpty();
    }
}
