using Microsoft.Extensions.Options;
using Pulse.DAL.Commands.LoginAttempts;
using Pulse.DAL.Queries.UserLoginAttempts;

namespace Pulse.BL.Features.Auth.Login.LoginLockout;

public class LoginLockoutService : ILoginLockoutService
{
    private readonly IUserLoginAttemptsQueries _userLoginLockoutQueries;
    private readonly TimeProvider _timeProvider;
    private readonly LoginLockoutOptions _options;

    public LoginLockoutService(
        IUserLoginAttemptsQueries userLoginLockoutQueries,
        TimeProvider timeProvider,
        IOptions<LoginLockoutOptions> options)
    {
        _timeProvider = timeProvider;
        _options = options.Value;
        _userLoginLockoutQueries = userLoginLockoutQueries;
    }
    public async Task<bool> IsUserAllowedAsync(Guid userId, CancellationToken ct)
    {
        UserLoginAttemptsRecord? loginAttempts = await _userLoginLockoutQueries.GetUserLoginAttemptsAsync(userId, ct);

        return loginAttempts?.LockedUntil is null ||
               loginAttempts.LockedUntil <= _timeProvider.GetUtcNow().UtcDateTime;
    }
}
