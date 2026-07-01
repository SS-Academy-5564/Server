using Microsoft.Extensions.Options;
using Pulse.DAL.Commands.LoginAttempts;
using Pulse.DAL.Queries.UserLoginAttempts;

namespace Pulse.BL.Features.Auth.Login.LoginLockout;

public class LoginLockoutService : ILoginLockoutService
{
    private readonly IUserLoginAttemptsQueries _userLoginLockoutQueries;
    private readonly IUserLoginAttemptsCommands _userLoginLockoutCommands;
    private readonly TimeProvider _timeProvider;
    private readonly LoginLockoutOptions _options;

    public LoginLockoutService(
        IUserLoginAttemptsQueries userLoginLockoutQueries,
        IUserLoginAttemptsCommands userLoginLockoutCommands,
        TimeProvider timeProvider,
        IOptions<LoginLockoutOptions> options)
    {
        _timeProvider = timeProvider;
        _options = options.Value;
        _userLoginLockoutQueries = userLoginLockoutQueries;
        _userLoginLockoutCommands = userLoginLockoutCommands;
    }

    public Task AddFailedAttemptAsync(Guid userId, CancellationToken ct)
    {
        DateTime now = _timeProvider.GetUtcNow().UtcDateTime;

        return _userLoginLockoutCommands.AddFailedAttemptAsync(
            userId,
            _options.MaxFailedAttempts,
            now,
            now.AddMinutes(_options.LockoutDurationMinutes),
            ct);
    }

    public async Task<bool> IsUserAllowedAsync(Guid userId, CancellationToken ct)
    {
        UserLoginAttemptsRecord? loginAttempts = await _userLoginLockoutQueries.GetUserLoginAttemptsAsync(userId, ct);

        return loginAttempts?.LockedUntil is null ||
               loginAttempts.LockedUntil <= _timeProvider.GetUtcNow().UtcDateTime;
    }

    public async Task ResetAttemptsAsync(Guid userId, CancellationToken ct)
    {
        await _userLoginLockoutCommands.ResetAttemptsAsync(userId, ct);
    }
}
