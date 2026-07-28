using FluentAssertions;
using FluentResults;
using Moq;
using Pulse.BL.Common.Security.Tokens;
using Pulse.BL.Features.Auth.Logout;
using Pulse.DAL.Commands.RefreshTokens;
using Pulse.DAL.Queries.RefreshTokens;

namespace Pulse.Tests.Unit.Features.Auth.Logout;

public class LogoutHandlerTests
{
    private readonly Mock<IRefreshTokenService> _refreshTokenServiceMock;
    private readonly Mock<IRefreshTokenQueries> _refreshTokenQueriesMock;
    private readonly Mock<IRefreshTokenCommands> _refreshTokenCommandsMock;
    private readonly TimeProvider _timeProvider;
    private readonly LogoutHandler _sut;

    public LogoutHandlerTests()
    {
        _refreshTokenServiceMock = new Mock<IRefreshTokenService>();
        _refreshTokenQueriesMock = new Mock<IRefreshTokenQueries>();
        _refreshTokenCommandsMock = new Mock<IRefreshTokenCommands>();
        _timeProvider = TimeProvider.System;

        _sut = new LogoutHandler(
            _refreshTokenServiceMock.Object,
            _refreshTokenQueriesMock.Object,
            _refreshTokenCommandsMock.Object,
            _timeProvider);
    }

    [Fact]
    public async Task HandleAsync_WhenTokenIsMissing_ReturnsOkWithoutDbCall()
    {
        // Arrange
        LogoutCommand command = new(null);

        // Act
        Result result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _refreshTokenQueriesMock.Verify(x => x.GetByTokenHashAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenTokenExists_RevokesTokenAndReturnsOk()
    {
        // Arrange
        string token = "valid_token";
        string hash = "hashed_token";
        LogoutCommand command = new(token);

        _refreshTokenServiceMock.Setup(x => x.ComputeHash(token)).Returns(hash);

        RefreshTokenRecord record = new(Guid.NewGuid(), Guid.NewGuid(), hash, Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(10), null, null, null, null);
        _refreshTokenQueriesMock.Setup(x => x.GetByTokenHashAsync(hash, It.IsAny<CancellationToken>())).ReturnsAsync(record);

        // Act
        Result result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _refreshTokenCommandsMock.Verify(x => x.UpdateAsync(It.Is<RefreshTokenRecord>(r => r.RevokedAt != null && r.RevocationReason == "Logout"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenTokenAlreadyRevoked_ReturnsOkWithoutUpdate()
    {
        // Arrange
        string token = "already_revoked_token";
        string hash = "hashed_token";
        LogoutCommand command = new(token);

        _refreshTokenServiceMock.Setup(x => x.ComputeHash(token)).Returns(hash);

        RefreshTokenRecord record = new(Guid.NewGuid(), Guid.NewGuid(), hash, Guid.NewGuid(), DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(10), null, DateTimeOffset.UtcNow, null, "Manual");
        _refreshTokenQueriesMock.Setup(x => x.GetByTokenHashAsync(hash, It.IsAny<CancellationToken>())).ReturnsAsync(record);

        // Act
        Result result = await _sut.HandleAsync(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _refreshTokenCommandsMock.Verify(x => x.UpdateAsync(It.IsAny<RefreshTokenRecord>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
