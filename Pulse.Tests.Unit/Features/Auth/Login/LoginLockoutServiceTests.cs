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
    private readonly LoginLockoutService _sut;

    public LoginLockoutServiceTests()
    {
        LoginLockoutOptions options = new()
        {
            MaxFailedAttempts = 5,
            LockoutDurationMinutes = 15
        };

        _sut = new LoginLockoutService(
            _queries.Object,
            _commands.Object,
            Options.Create(options));
    }

    [Fact]
    public async Task AddFailedAttemptAsync_WhenCalled_PassesPolicyToCommandAsync()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        // Act
        await _sut.AddFailedAttemptAsync(userId, CancellationToken.None);

        // Assert
        _commands.Verify(x => x.AddFailedAttemptAsync(
            userId,
            5,
            15,
            CancellationToken.None), Times.Once);
    }
}
