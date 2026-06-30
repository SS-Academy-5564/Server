using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.API.Features.Users.Me;
using Pulse.API.Responses;
using Pulse.BL.Common.Errors;
using Pulse.BL.Features.Users.Me;

namespace Pulse.Tests.Unit.Features.Users.Me;

public class CurrentUserControllerTests
{
    private readonly Mock<ICurrentUserHandler> _handlerMock;
    private readonly CurrentUserController _sut;

    public CurrentUserControllerTests()
    {
        _handlerMock = new();
        _sut = new CurrentUserController(_handlerMock.Object);
    }

    [Fact]
    public async Task GetCurrentUser_WhenSuccess_Returns200()
    {
        var profile = new UserProfileResult(Guid.NewGuid(), "user@example.com", "John", "Doe", DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

        _handlerMock.Setup(x => x.GetCurrentUserAsync(It.IsAny<CancellationToken>()))
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
        _handlerMock.Setup(x => x.GetCurrentUserAsync(It.IsAny<CancellationToken>()))
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
        _handlerMock.Setup(x => x.GetCurrentUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(new NotFoundError("User not found.")));

        IActionResult result = await _sut.GetCurrentUserAsync(CancellationToken.None);

        ObjectResult obj = result.Should().BeAssignableTo<ObjectResult>().Subject;
        obj.StatusCode.Should().Be(404);

        ApiResponse response = obj.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Success.Should().BeFalse();
        response.Errors[0].Code.Should().Be(AppError.Codes.NotFound);
    }
}
