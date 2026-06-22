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
        _userQueriesMock = new();
        _passwordHasherMock = new();
        _jwtTokenGeneratorMock = new();

        _sut = new LoginHandler(
            _userQueriesMock.Object,
            _passwordHasherMock.Object,
            _jwtTokenGeneratorMock.Object,
            new Mock<Microsoft.Extensions.Logging.ILogger<LoginHandler>>().Object
        );
    }

    [Fact]
    public async Task LoginAsync_WhenCredentialsValid_ReturnsTokenAsync()
    {
        string email = "user@example.com";
        string password = "ValidPassword123";
        string passwordHash = "$2a$11$hashed_password";
        Guid userId = Guid.NewGuid();
        string roleName = "User";
        Guid organizationId = Guid.NewGuid();
        string accessToken = "jwt_token_here";
        DateTimeOffset expiresAt = DateTimeOffset.UtcNow.AddHours(1);

        UserAuthRecord userRecord = new()
        {
            Id = userId,
            Email = email,
            PasswordHash = passwordHash,
            RoleName = roleName,
            OrganizationId = organizationId
        };

        LoginCommand command = new()
        {
            Email = email,
            Password = password
        };

        _userQueriesMock
            .Setup(x => x.GetByEmailForAuthAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userRecord);

        _passwordHasherMock
            .Setup(x => x.VerifyHashedPassword(passwordHash, password))
            .Returns(true);

        _jwtTokenGeneratorMock
            .Setup(x => x.GenerateToken(userId, roleName, organizationId))
            .Returns(new GeneratedJwtToken(accessToken, expiresAt));

        Result<LoginResult> result =
            await _sut.LoginAsync(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be(accessToken);
        result.Value.ExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public async Task LoginAsync_WhenUserDoesNotExist_ReturnsUnauthorizedErrorAsync()
    {
        string email = "notfound@example.com";
        string password = "Password123";

        LoginCommand command = new()
        {
            Email = email,
            Password = password
        };

        _userQueriesMock
            .Setup(x => x.GetByEmailForAuthAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserAuthRecord?)null);

        Result<LoginResult> result =
            await _sut.LoginAsync(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle();

        UnauthorizedError error =
            result.Errors.First().Should().BeOfType<UnauthorizedError>().Subject;

        error.Message.Should().Be("Invalid email or password.");
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordInvalid_ReturnsUnauthorizedErrorAsync()
    {
        string email = "user@example.com";
        string password = "InvalidPassword";
        string passwordHash = "$2a$11$hashed_password";

        UserAuthRecord userRecord = new()
        {
            Id = Guid.NewGuid(),
            Email = email,
            PasswordHash = passwordHash,
            RoleName = "User",
            OrganizationId = Guid.NewGuid()
        };

        LoginCommand command = new()
        {
            Email = email,
            Password = password
        };

        _userQueriesMock
            .Setup(x => x.GetByEmailForAuthAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userRecord);

        _passwordHasherMock
            .Setup(x => x.VerifyHashedPassword(passwordHash, password))
            .Returns(false);

        Result<LoginResult> result =
            await _sut.LoginAsync(command, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle();

        UnauthorizedError error =
            result.Errors.First().Should().BeOfType<UnauthorizedError>().Subject;

        error.Message.Should().Be("Invalid email or password.");
    }
}
