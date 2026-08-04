using System.Net;
using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using Pulse.API.Features.Auth.Login;
using Pulse.API.Responses;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security.Tokens;
using Pulse.BL.Features.Auth.Login;

namespace Pulse.Tests.Unit.Features.Auth.Login;

public class LoginControllerTests
{
    private readonly Mock<IAsyncHandler<LoginCommand, Result<LoginResult>>> _handlerMock;
    private readonly Mock<IOptions<RefreshTokenOptions>> _refreshTokenOptionsMock;
    private readonly Mock<IHostEnvironment> _environmentMock;
    private readonly TimeProvider _timeProvider;
    private readonly LoginController _sut;

    public LoginControllerTests()
    {
        _handlerMock = new();
        RefreshTokenOptions options = new() { ExpirationDays = 14 };
        _refreshTokenOptionsMock = new();
        _refreshTokenOptionsMock.Setup(x => x.Value).Returns(options);
        _environmentMock = new();
        _environmentMock.Setup(x => x.EnvironmentName).Returns(Environments.Development);
        _timeProvider = TimeProvider.System;

        _sut = new LoginController(
            _handlerMock.Object,
            _refreshTokenOptionsMock.Object,
            _timeProvider,
            _environmentMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task Login_WhenSuccess_Returns200Async()
    {
        // Arrange
        LoginRequest request = new("user@example.com", "ValidPassword123");

        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddHours(1);
        LoginResult loginResult = new("jwt_token_here", expiresAt, "raw_refresh_token");

        _handlerMock
            .Setup(x => x.HandleAsync(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(loginResult));

        // Act
        IActionResult result = await _sut.LoginAsync(request, CancellationToken.None);

        // Assert
        OkObjectResult okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.StatusCode.Should().Be(200);

        ApiResponse<LoginResponse> response = okResult.Value.Should().BeOfType<ApiResponse<LoginResponse>>().Subject;
        response.Success.Should().BeTrue();
        response.Errors.Should().BeEmpty();
        response.Data.Should().NotBeNull();
        response.Data!.AccessToken.Should().Be(loginResult.AccessToken);
        response.Data.ExpiresAt.Should().Be(loginResult.ExpiresAt);
        _sut.Response.Headers.SetCookie.ToString().Should().Contain("httponly").And.Contain("samesite=lax");
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
    public async Task Login_WhenProduction_IssuesSecureCrossSiteRefreshCookieAsync()
    {
        // Arrange
        _environmentMock.Setup(x => x.EnvironmentName).Returns(Environments.Production);
        LoginRequest request = new("user@example.com", "ValidPassword123");
        LoginResult loginResult = new(
            "jwt_token_here",
            DateTimeOffset.UtcNow.AddHours(1),
            "raw_refresh_token");

        _handlerMock
            .Setup(x => x.HandleAsync(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(loginResult));

        // Act
        await _sut.LoginAsync(request, CancellationToken.None);

        // Assert
        string setCookie = _sut.Response.Headers.SetCookie.ToString().ToLowerInvariant();
        setCookie.Should().Contain("httponly").And.Contain("secure").And.Contain("samesite=none");
    }

    [Fact]
    public async Task Login_WhenHandlerCalled_PassesCorrectCommandAsync()
    {
        // Arrange
        LoginRequest request = new("user@example.com", "Password123");

        LoginResult loginResult = new("token", DateTimeOffset.UtcNow.AddHours(1), "raw_refresh_token");

        _handlerMock
            .Setup(x => x.HandleAsync(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(loginResult));

        _sut.ControllerContext.HttpContext.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");

        // Act
        await _sut.LoginAsync(request, CancellationToken.None);

        // Assert
        _handlerMock.Verify(
            x => x.HandleAsync(
                It.Is<LoginCommand>(cmd =>
                    cmd.Email == request.Email &&
                    cmd.Password == request.Password &&
                    cmd.Identifier == "127.0.0.1"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
