using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Security.Tokens;
using Pulse.BL.Features.Auth.Login;
using Pulse.BL.Features.Auth.Refresh;
using Pulse.DAL.Commands.RefreshTokens;
using Pulse.DAL.Queries.RefreshTokens;
using Pulse.DAL.Queries.Users;

namespace Pulse.Tests.Unit.Features.Auth.Refresh;

public class RefreshHandlerTests
{
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<IRefreshTokenQueries> _refreshTokenQueriesMock;
    private readonly Mock<IRefreshTokenCommands> _refreshTokenCommandsMock;
    private readonly Mock<IUserQueries> _userQueriesMock;
    private readonly Mock<IJwtTokenGenerator> _jwtTokenGeneratorMock;
    private readonly Mock<IOptions<RefreshTokenOptions>> _optionsMock;
    private readonly TimeProvider _timeProvider;
    private readonly RefreshHandler _sut;

    public RefreshHandlerTests()
    {
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _refreshTokenQueriesMock = new Mock<IRefreshTokenQueries>();
        _refreshTokenCommandsMock = new Mock<IRefreshTokenCommands>();
        _userQueriesMock = new Mock<IUserQueries>();
        _jwtTokenGeneratorMock = new Mock<IJwtTokenGenerator>();
        _optionsMock = new Mock<IOptions<RefreshTokenOptions>>();
        _optionsMock.Setup(x => x.Value).Returns(new RefreshTokenOptions { ExpirationDays = 14 });
        _timeProvider = TimeProvider.System;

        _sut = new RefreshHandler(
            _refreshTokenServiceMock.Object,
            _refreshTokenQueriesMock.Object,
            _refreshTokenCommandsMock.Object,
            _userQueriesMock.Object,
            _jwtTokenGeneratorMock.Object,
            _optionsMock.Object,
            _timeProvider,
            new Mock<ILogger<RefreshHandler>>().Object);
    }

    [Fact]
    public async Task HandleAsync_WhenTokenIsUnknown_ReturnsUnauthorizedError()
    {
        // Arrange
        RefreshCommand command = new("unknown_token");
        _refreshTokenServiceMock.Setup(x => x.ComputeHash(command.RefreshToken)).Returns("hashed_unknown");
        _refreshTokenQueriesMock.Setup(x => x.GetByTokenHashAsync("hashed_unknown", It.IsAny<CancellationToken>())).ReturnsAsync((RefreshTokenRecord?)null);

        // Act
        Result<LoginResult> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Should().BeOfType<UnauthorizedError>();
    }

    [Fact]
    public async Task HandleAsync_WhenTokenIsExpired_ReturnsUnauthorizedError()
    {
        // Arrange
        RefreshCommand command = new("expired_token");
        string hash = "hashed_expired";
        _refreshTokenServiceMock.Setup(x => x.ComputeHash(command.RefreshToken)).Returns(hash);
        RefreshTokenRecord expiredRecord = new(Guid.NewGuid(), Guid.NewGuid(), hash, Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(-20), DateTimeOffset.UtcNow.AddDays(-5), null, null, null, null);
        _refreshTokenQueriesMock.Setup(x => x.GetByTokenHashAsync(hash, It.IsAny<CancellationToken>())).ReturnsAsync(expiredRecord);

        // Act
        Result<LoginResult> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Should().BeOfType<UnauthorizedError>();
    }

    [Fact]
    public async Task HandleAsync_WhenTokenIsRevoked_ReturnsUnauthorizedError()
    {
        // Arrange
        RefreshCommand command = new("revoked_token");
        string hash = "hashed_revoked";
        _refreshTokenServiceMock.Setup(x => x.ComputeHash(command.RefreshToken)).Returns(hash);
        RefreshTokenRecord revokedRecord = new(Guid.NewGuid(), Guid.NewGuid(), hash, Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(10), null, DateTimeOffset.UtcNow, null, "ManualRevocation");
        _refreshTokenQueriesMock.Setup(x => x.GetByTokenHashAsync(hash, It.IsAny<CancellationToken>())).ReturnsAsync(revokedRecord);

        // Act
        Result<LoginResult> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Should().BeOfType<UnauthorizedError>();
    }

    [Fact]
    public async Task HandleAsync_WhenTokenIsReused_RevokesFamilyAndReturnsUnauthorizedError()
    {
        // Arrange
        RefreshCommand command = new("reused_token");
        string hash = "hashed_reused";
        Guid familyId = Guid.NewGuid();
        _refreshTokenServiceMock.Setup(x => x.ComputeHash(command.RefreshToken)).Returns(hash);
        RefreshTokenRecord reusedRecord = new(Guid.NewGuid(), Guid.NewGuid(), hash, familyId, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(10), DateTimeOffset.UtcNow.AddDays(-1), null, Guid.NewGuid(), null);
        _refreshTokenQueriesMock.Setup(x => x.GetByTokenHashAsync(hash, It.IsAny<CancellationToken>())).ReturnsAsync(reusedRecord);

        // Act
        Result<LoginResult> result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Should().BeOfType<UnauthorizedError>();
        _refreshTokenCommandsMock.Verify(x => x.RevokeFamilyAsync(familyId, "RefreshTokenReuse", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailIsNotVerified_RevokesUserTokensAndReturnsForbidden()
    {
        const string rawToken = "active_token";
        const string tokenHash = "hashed_active";
        Guid userId = Guid.NewGuid();
        RefreshTokenRecord currentRecord = new(
            Guid.NewGuid(),
            userId,
            tokenHash,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(10));
        UserAuthRecord user = new(
            userId,
            "unverified@example.com",
            "password-hash",
            Guid.NewGuid(),
            "User",
            "Test Organization",
            0,
            false,
            null);
        _refreshTokenServiceMock.Setup(x => x.ComputeHash(rawToken)).Returns(tokenHash);
        _refreshTokenQueriesMock
            .Setup(x => x.GetByTokenHashAsync(tokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentRecord);
        _userQueriesMock
            .Setup(x => x.GetByIdForAuthAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        Result<LoginResult> result = await _sut.HandleAsync(
            new RefreshCommand(rawToken),
            CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<EmailNotVerifiedError>();
        _refreshTokenCommandsMock.Verify(
            x => x.RevokeAllForUserAsync(userId, "EmailNotVerified", CancellationToken.None),
            Times.Once);
        _refreshTokenCommandsMock.Verify(
            x => x.RotateAsync(
                It.IsAny<RefreshTokenRecord>(),
                It.IsAny<RefreshTokenRecord>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
        _jwtTokenGeneratorMock.Verify(
            x => x.GenerateToken(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenEmailIsVerified_RotatesTokensAndReturnsSuccess()
    {
        const string currentRawToken = "current_token";
        const string currentTokenHash = "hashed_current";
        const string newRawToken = "new_token";
        const string newTokenHash = "hashed_new";
        Guid userId = Guid.NewGuid();
        Guid organizationId = Guid.NewGuid();
        DateTimeOffset now = _timeProvider.GetUtcNow();
        RefreshTokenRecord currentRecord = new(
            Guid.NewGuid(),
            userId,
            currentTokenHash,
            Guid.NewGuid(),
            now.AddDays(-1),
            now.AddDays(10));
        UserAuthRecord user = new(
            userId,
            "verified@example.com",
            "password-hash",
            organizationId,
            "User",
            "Test Organization",
            0,
            false,
            now.AddDays(-2));
        GeneratedJwtToken accessToken = new("access_token", now.AddMinutes(15));
        _refreshTokenServiceMock.Setup(x => x.ComputeHash(currentRawToken)).Returns(currentTokenHash);
        _refreshTokenServiceMock.Setup(x => x.GenerateToken()).Returns(newRawToken);
        _refreshTokenServiceMock.Setup(x => x.ComputeHash(newRawToken)).Returns(newTokenHash);
        _refreshTokenQueriesMock
            .Setup(x => x.GetByTokenHashAsync(currentTokenHash, It.IsAny<CancellationToken>()))
            .ReturnsAsync(currentRecord);
        _userQueriesMock
            .Setup(x => x.GetByIdForAuthAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        _refreshTokenCommandsMock
            .Setup(x => x.RotateAsync(
                It.IsAny<RefreshTokenRecord>(),
                It.Is<RefreshTokenRecord>(record => record.TokenHash == newTokenHash),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _jwtTokenGeneratorMock
            .Setup(x => x.GenerateToken(userId, "User", organizationId, "Test Organization"))
            .Returns(accessToken);

        Result<LoginResult> result = await _sut.HandleAsync(
            new RefreshCommand(currentRawToken),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be(accessToken.Token);
        result.Value.RefreshToken.Should().Be(newRawToken);
        _refreshTokenCommandsMock.Verify(
            x => x.RevokeAllForUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
