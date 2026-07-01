using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Pulse.BL.Features.Auth.Login.LoginLockout;
using Pulse.DAL.Commands.LoginAttempts;
using Pulse.DAL.Queries.UserLoginAttempts;

namespace Pulse.Tests.Unit.Features.Auth.Login;

public class LoginLockoutServiceTests
{
    private readonly Mock<IUserLoginAttemptsQueries> _queries = new();
    private readonly Mock<IUserLoginAttemptsCommands> _commands = new();
    private readonly LoginLockoutOptions _options = new()
    {
        MaxFailedAttempts = 5,
        LockoutDurationMinutes = 15
    };

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IsUserAllowedAsync_ReturnsQueryResultAsync(bool expected)
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        _queries
            .Setup(x => x.IsUserAllowedAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        bool result = await CreateSut().IsUserAllowedAsync(
            userId,
            CancellationToken.None);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task AddFailedAttemptAsync_PassesPolicyToCommandAsync()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        // Act
        await CreateSut().AddFailedAttemptAsync(userId, CancellationToken.None);

        // Assert
        _commands.Verify(x => x.AddFailedAttemptAsync(
            userId,
            _options.MaxFailedAttempts,
            _options.LockoutDurationMinutes,
            CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task ResetAttemptsAsync_DelegatesToCommandAsync()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        using CancellationTokenSource cts = new();

        // Act
        await CreateSut().ResetAttemptsAsync(userId, cts.Token);

        // Assert
        _commands.Verify(
            x => x.ResetAttemptsAsync(userId, cts.Token),
            Times.Once);
    }

    private LoginLockoutService CreateSut()
    {
        return new LoginLockoutService(
            _queries.Object,
            _commands.Object,
            Options.Create(_options));
    }
}
