using FluentAssertions;
using FluentResults;
using Moq;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Security.Passwords;
using Pulse.BL.Common.Security.Tokens;
using Pulse.BL.Features.Auth.Login;
using Pulse.BL.Features.Auth.Login.LoginLockout;
using Pulse.DAL.Queries.Users;

namespace Pulse.Tests.Unit.Features.Auth.Login;

public class LoginHandlerTests
{
    private readonly Mock<IUserQueries> _userQueriesMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly Mock<ILoginLockoutService> _loginLockoutServiceMock;
    private readonly LoginHandler _sut;

    public LoginHandlerTests()
    {
        _userQueriesMock = new();
        _passwordHasherMock = new();
        _jwtTokenGeneratorMock = new();
        _loginLockoutServiceMock = new();

        _sut = new LoginHandler(
            _userQueriesMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object,
            _loginLockoutServiceMock.Object,
            new Mock<Microsoft.Extensions.Logging.ILogger<LoginHandler>>().Object);
    }

    [Fact]
    public async Task HandleAsync_WhenCredentialsValid_ReturnsTokenAsync()
    {
        // Arrange
        string email = "user@example.com";
        string password = "ValidPassword123";
        string passwordHash = "$2a$11$hashed_password";
        Guid userId = Guid.NewGuid();
        string roleName = "User";
        Guid organizationId = Guid.NewGuid();
        string accessToken = "jwt_token_here";
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        UserAuthRecord userRecord = new(
            userId,
            email,
            passwordHash,
            organizationId,
            roleName,
            "Test Organization",
            0,
            false);

        LoginCommand command = new(email, password);

        _userQueriesMock
            .Setup(x => x.GetByEmailForAuthAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userRecord);

        _passwordHasherMock
            .Setup(x => x.VerifyHashedPassword(passwordHash, password))
            .Returns(true);

        _jwtTokenGeneratorMock
            .Setup(x => x.GenerateToken(userId, roleName, organizationId, "Test Organization"))
            .Returns(new GeneratedJwtToken(accessToken, expiresAt));

        // Act
        Result<LoginResult> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be(accessToken);
        result.Value.ExpiresAt.Should().Be(expiresAt);
        _loginLockoutServiceMock.Verify(
            x => x.ResetAttemptsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _loginLockoutServiceMock.Verify(
            x => x.IsUserAllowedAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenCredentialsValidAndPreviousAttemptsExist_ResetsAttemptsAsync()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid organizationId = Guid.NewGuid();
        UserAuthRecord userRecord = new(
            userId,
            "user@example.com",
            "password-hash",
            organizationId,
            "User",
            "Test Organization",
            2,
            false);

        _userQueriesMock
            .Setup(x => x.GetByEmailForAuthAsync(userRecord.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userRecord);
        _passwordHasherMock
            .Setup(x => x.VerifyHashedPassword(userRecord.PasswordHash, "Password123"))
            .Returns(true);
        _jwtTokenGeneratorMock
            .Setup(x => x.GenerateToken(userId, userRecord.RoleName, organizationId, userRecord.OrganizationName))
            .Returns(new GeneratedJwtToken("token", DateTimeOffset.UtcNow.AddHours(1)));

        // Act
        Result<LoginResult> result = await _sut.HandleAsync(
            new LoginCommand(userRecord.Email, "Password123"),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _loginLockoutServiceMock.Verify(
            x => x.ResetAttemptsAsync(userId, CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenUserDoesNotExist_ReturnsUnauthorizedErrorAsync()
    {
        // Arrange
        string email = "notfound@example.com";
        string password = "Password123";
        LoginCommand command = new(email, password);

        _userQueriesMock
            .Setup(x => x.GetByEmailForAuthAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAuthRecord?)null);

        // Act
        Result<LoginResult> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle();

        UnauthorizedError error = result.Errors.First().Should().BeOfType<UnauthorizedError>().Subject;
        error.Message.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task HandleAsync_WhenPasswordInvalid_ReturnsUnauthorizedErrorAsync()
    {
        // Arrange
        string email = "user@example.com";
        string password = "InvalidPassword";
        string passwordHash = "$2a$11$hashed_password";
        Guid organizationId = Guid.NewGuid();

        UserAuthRecord userRecord = new(
            Guid.NewGuid(),
            email,
            passwordHash,
            organizationId,
            "User",
            "Test Organization",
            0,
            false);

        LoginCommand command = new(email, password);

        _userQueriesMock
            .Setup(x => x.GetByEmailForAuthAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userRecord);

        _passwordHasherMock
            .Setup(x => x.VerifyHashedPassword(passwordHash, password))
            .Returns(false);

        // Act
        Result<LoginResult> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle();

        UnauthorizedError error = result.Errors.First().Should().BeOfType<UnauthorizedError>().Subject;
        error.Message.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task LoginAsync_WhenUserLocked_ReturnsGenericUnauthorizedErrorAsync()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        const string email = "user@example.com";
        UserAuthRecord userRecord = new(
            userId,
            email,
            "hash",
            Guid.NewGuid(),
            "User",
            "Test Organization",
            3,
            true);

        _userQueriesMock
            .Setup(x => x.GetByEmailForAuthAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userRecord);

        // Act
        Result<LoginResult> result = await _sut.HandleAsync(
            new LoginCommand(email, "Password123"),
            CancellationToken.None);

        // Assert
        result.Errors.Single().Should().BeOfType<UnauthorizedError>()
            .Which.Message.Should().Be("Invalid email or password.");
        _passwordHasherMock.Verify(
            x => x.VerifyHashedPassword(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }
}
