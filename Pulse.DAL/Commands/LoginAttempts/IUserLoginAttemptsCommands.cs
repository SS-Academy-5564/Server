using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.LoginAttempts;

/// <summary>
/// Defines database commands for mutating user login attempt state.
/// </summary>
public interface IUserLoginAttemptsCommands : ICommands
{
    Task AddFailedAttemptAsync(
        Guid userId,
        int maxFailedAttempts,
        DateTime now,
        DateTime lockedUntil,
        CancellationToken ct);

    Task ResetAttemptsAsync(Guid userId, CancellationToken ct);
}
