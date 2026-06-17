using FluentAssertions;
using FluentResults;
using Moq;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Security.Passwords;
using Pulse.BL.Common.Security.Tokens;
using Pulse.BL.Features.Auth.Login;
using Pulse.DAL.Queries.Users;

namespace Pulse.Tests.Unit.Features.Auth.Login;

public class LoginHandlerTests
{
    private readonly Mock<IUserQueries> _userQueriesMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly LoginHandler _sut;

    public LoginHandlerTests()
    {
        _userQueriesMock = new Mock<IUserQueries>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();

        _sut = new LoginHandler(
            _userQueriesMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object,
            new Mock<Microsoft.Extensions.Logging.ILogger<LoginHandler>>().Object);
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsValid_ReturnsToken()
    {
        // Arrange
        var email = "user@example.com";
        var password = "ValidPassword123";
        var passwordHash = "$2a$11$hashed_password";
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();
        var organizationId = Guid.NewGuid();
        var accessToken = "jwt_token_here";
        var expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        var userRecord = new UserAuthRecord
        {
            Id = userId,
            Email = email,
            PasswordHash = passwordHash,
            RoleId = roleId,
            OrganizationId = organizationId
        };

        var command = new LoginCommand { Email = email, Password = password };

        _userQueriesMock
            .Setup(x => x.GetByEmailForAuthAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userRecord);

        _passwordHasherMock
            .Setup(x => x.VerifyHashedPassword(passwordHash, password))
            .Returns(true);

        _jwtTokenGeneratorMock
            .Setup(x => x.GenerateToken(userId, roleId, organizationId, out expiresAt))
            .Returns(accessToken);

        // Act
        var result = await _sut.LoginAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be(accessToken);
        result.Value.ExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ReturnsUnauthorizedError()
    {
        // Arrange
        var email = "notfound@example.com";
        var password = "Password123";
        var command = new LoginCommand { Email = email, Password = password };

        _userQueriesMock
            .Setup(x => x.GetByEmailForAuthAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAuthRecord?)null);

        // Act
        var result = await _sut.LoginAsync(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        var error = result.Errors.First().Should().BeOfType<UnauthorizedError>().Subject;
        error.Message.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordInvalid_ReturnsUnauthorizedError()
    {
        // Arrange
        var email = "user@example.com";
        var password = "InvalidPassword";
        var passwordHash = "$2a$11$hashed_password";
        var userRecord = new UserAuthRecord
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            RoleId = Guid.NewGuid(),
            OrganizationId = Guid.NewGuid()
        };

        var command = new LoginCommand { Email = email, Password = password };

        _userQueriesMock
            .Setup(x => x.GetByEmailForAuthAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userRecord);

        _passwordHasherMock
            .Setup(x => x.VerifyHashedPassword(passwordHash, password))
            .Returns(false);

        // Act
        var result = await _sut.LoginAsync(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle();
        var error = result.Errors.First().Should().BeOfType<UnauthorizedError>().Subject;
        error.Message.Should().Be("Invalid email or password.");
    }
}
