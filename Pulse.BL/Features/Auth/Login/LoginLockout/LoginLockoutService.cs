using Microsoft.Extensions.Options;
using Pulse.DAL.Commands.LoginAttempts;

namespace Pulse.BL.Features.Auth.Login.LoginLockout;

public class LoginLockoutService : ILoginLockoutService
{
    private readonly IUserLoginAttemptsCommands _userLoginLockoutCommands;
    private readonly TimeProvider _timeProvider;
    private readonly LoginLockoutOptions _options;

    public LoginLockoutService(
        IUserLoginAttemptsCommands userLoginLockoutCommands,
        TimeProvider timeProvider,
        IOptions<LoginLockoutOptions> options)
    {
        _timeProvider = timeProvider;
        _options = options.Value;
        _userLoginLockoutCommands = userLoginLockoutCommands;
    }
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

    public async Task ResetAttemptsAsync(Guid userId, CancellationToken ct)
    {
        await _userLoginLockoutCommands.ResetAttemptsAsync(userId, ct);
    }
}
