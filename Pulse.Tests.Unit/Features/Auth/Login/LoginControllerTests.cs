using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.API.Features.Auth.Login;
using Pulse.BL.Common.Errors;
using Pulse.BL.Features.Auth.Login;

namespace Pulse.Tests.Unit.Features.Auth.Login;

public class LoginControllerTests
{
    private readonly Mock<ILoginHandler> _handlerMock;
    private readonly LoginController _sut;

    public LoginControllerTests()
    {
        _handlerMock = new();
        _sut = new LoginController(_handlerMock.Object);
    }

    [Fact]
    public async Task Login_WhenSuccess_Returns200Async()
    {
        LoginRequest request = new()
        {
            Email = "user@example.com",
            Password = "ValidPassword123"
        };

        LoginResult loginResult = new()
        {
            AccessToken = "jwt_token_here",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };

        _handlerMock
            .Setup(x => x.LoginAsync(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(loginResult));

        IActionResult result =
            await _sut.LoginAsync(request, CancellationToken.None);

        OkObjectResult okResult =
            result.Should().BeOfType<OkObjectResult>().Subject;

        okResult.StatusCode.Should().Be(200);
        okResult.Value.Should().Be(loginResult);
    }

    [Fact]
    public async Task Login_WhenUnauthorized_Returns401Async()
    {
        LoginRequest request = new()
        {
            Email = "invalid@example.com",
            Password = "InvalidPassword"
        };

        _handlerMock
            .Setup(x => x.LoginAsync(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(new UnauthorizedError("Invalid email or password.")));

        IActionResult result =
            await _sut.LoginAsync(request, CancellationToken.None);

        ObjectResult objectResult =
            result.Should().BeOfType<ObjectResult>().Subject;

        objectResult.StatusCode.Should().Be(401);

        ProblemDetails problem =
            objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;

        problem.Detail.Should().Be("Invalid email or password.");
        problem.Extensions["code"].Should().Be(AppError.Codes.Unauthorized);
    }

    [Fact]
    public async Task Login_WhenHandlerCalled_PassesCorrectCommandAsync()
    {
        LoginRequest request = new()
        {
            Email = "user@example.com",
            Password = "Password123"
        };

        LoginResult loginResult = new()
        {
            AccessToken = "token",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(1)
        };

        _handlerMock
            .Setup(x => x.LoginAsync(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(loginResult));

        await _sut.LoginAsync(request, CancellationToken.None);

        _handlerMock.Verify(
            x => x.LoginAsync(
                It.Is<LoginCommand>(cmd =>
                    cmd.Email == request.Email &&
                    cmd.Password == request.Password),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
