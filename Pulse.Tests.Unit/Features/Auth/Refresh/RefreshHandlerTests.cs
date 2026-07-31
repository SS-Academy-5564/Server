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
}
