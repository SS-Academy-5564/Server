using Microsoft.Extensions.Options;
using Pulse.DAL.Commands.LoginAttempts;

namespace Pulse.BL.Features.Auth.Login.LoginLockout;

public class LoginLockoutService : ILoginLockoutService
{
    private readonly IUserLoginAttemptsCommands _userLoginLockoutCommands;
    private readonly TimeProvider _timeProvider;
    private readonly LoginLockoutOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginLockoutService"/> class.
    /// </summary>
    /// <param name="userLoginLockoutCommands">The login attempt database commands.</param>
    /// <param name="timeProvider">The provider used to obtain the current UTC time.</param>
    /// <param name="options">The configured login lockout options.</param>
    public LoginLockoutService(
        IUserLoginAttemptsCommands userLoginLockoutCommands,
        TimeProvider timeProvider,
        IOptions<LoginLockoutOptions> options)
    {
        _timeProvider = timeProvider;
        _options = options.Value;
        _userLoginLockoutCommands = userLoginLockoutCommands;
    }
    /// <inheritdoc/>
    public Task<bool> TryReserveLoginAttemptAsync(Guid userId, CancellationToken ct)
    {
        DateTime now = _timeProvider.GetUtcNow().UtcDateTime;

        return _userLoginLockoutCommands.TryReserveLoginAttemptAsync(
            userId,
            _options.MaxAttempts,
            now,
            now.AddMinutes(_options.LockoutDurationMinutes),
            ct);
    }

    /// <inheritdoc/>
    public async Task ResetAttemptsAsync(Guid userId, CancellationToken ct)
    {
        await _userLoginLockoutCommands.ResetAttemptsAsync(userId, ct);
    }
}
