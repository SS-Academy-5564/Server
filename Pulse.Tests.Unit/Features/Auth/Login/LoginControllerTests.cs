using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.API.Features.Auth.Login;
using Pulse.API.Responses;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.Auth.Login;

namespace Pulse.Tests.Unit.Features.Auth.Login;

public class LoginControllerTests
{
    private readonly Mock<IAsyncHandler<LoginCommand, Result<LoginResult>>> _handlerMock;
    private readonly LoginController _sut;

    public LoginControllerTests()
    {
        _handlerMock = new();
        _sut = new LoginController(
            _handlerMock.Object,
            new Mock<ILogger<LoginController>>().Object);
    }

    [Fact]
    public async Task Login_WhenSuccess_Returns200Async()
    {
        // Arrange
        LoginRequest request = new("user@example.com", "ValidPassword123");

        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        LoginResult loginResult = new("jwt_token_here", expiresAt);

        _handlerMock
            .Setup(x => x.HandleAsync(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(loginResult));

        // Act
        IActionResult result = await _sut.LoginAsync(request, CancellationToken.None);

        // Assert
        OkObjectResult okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        ApiResponse<LoginResult> response = okResult.Value.Should().BeOfType<ApiResponse<LoginResult>>().Subject;
        response.Success.Should().BeTrue();
        response.Errors.Should().BeEmpty();
        response.Data.Should().BeEquivalentTo(loginResult);
    }

    [Fact]
    public async Task Login_WhenUnauthorized_Returns401Async()
    {
        // Arrange
        LoginRequest request = new("invalid@example.com", "InvalidPassword");

        _handlerMock
            .Setup(x => x.HandleAsync(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(new UnauthorizedError("Invalid email or password.")));

        // Act
        IActionResult result = await _sut.LoginAsync(request, CancellationToken.None);

        // Assert
        ObjectResult objectResult = result.Should().BeAssignableTo<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(401);

        ApiResponse response = objectResult.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Success.Should().BeFalse();
        response.Errors.Should().NotBeEmpty();
        response.Errors[0].Message.Should().Be("Invalid email or password.");
        response.Errors[0].Code.Should().Be(AppError.Codes.Unauthorized);
    }

    [Fact]
    public async Task Login_WhenHandlerCalled_PassesCorrectCommandAsync()
    {
        // Arrange
        LoginRequest request = new("user@example.com", "Password123");

        LoginResult loginResult = new("token", DateTimeOffset.UtcNow.AddHours(1));

        _handlerMock
            .Setup(x => x.HandleAsync(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(loginResult));

        // Act
        await _sut.LoginAsync(request, CancellationToken.None);

        // Assert
        _handlerMock.Verify(
            x => x.HandleAsync(
                It.Is<LoginCommand>(cmd =>
                    cmd.Email == request.Email &&
                    cmd.Password == request.Password),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
