using Microsoft.Extensions.Options;
using Pulse.DAL.Commands.LoginAttempts;
using Pulse.DAL.Queries.UserLoginAttempts;

namespace Pulse.BL.Features.Auth.Login.LoginLockout;

/// <inheritdoc cref="ILoginLockoutService"/>
public class LoginLockoutService : ILoginLockoutService
{
    private readonly IUserLoginAttemptsQueries _userLoginLockoutQueries;
    private readonly IUserLoginAttemptsCommands _userLoginLockoutCommands;
    private readonly LoginLockoutOptions _options;

    public LoginLockoutService(
        IUserLoginAttemptsQueries userLoginLockoutQueries,
        IUserLoginAttemptsCommands userLoginLockoutCommands,
        IOptions<LoginLockoutOptions> options)
    {
        _options = options.Value;
        _userLoginLockoutQueries = userLoginLockoutQueries;
        _userLoginLockoutCommands = userLoginLockoutCommands;
    }

    /// <inheritdoc/>
    public Task AddFailedAttemptAsync(Guid userId, CancellationToken ct)
    {
        return _userLoginLockoutCommands.AddFailedAttemptAsync(
            userId,
            _options.MaxFailedAttempts,
            _options.LockoutDurationMinutes,
            ct);
    }

    /// <inheritdoc/>
    public Task<bool> IsUserAllowedAsync(Guid userId, CancellationToken ct)
    {
        return _userLoginLockoutQueries.IsUserAllowedAsync(userId, ct);
    }

    /// <inheritdoc/>
    public async Task ResetAttemptsAsync(Guid userId, CancellationToken ct)
    {
        await _userLoginLockoutCommands.ResetAttemptsAsync(userId, ct);
    }
}
