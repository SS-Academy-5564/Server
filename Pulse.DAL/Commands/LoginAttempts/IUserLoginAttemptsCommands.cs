using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.LoginAttempts;

/// <summary>
/// Defines database commands for mutating user login attempt state.
/// </summary>
public interface IUserLoginAttemptsCommands : ICommands
{
    /// <summary>
    /// Atomically records a failed attempt and applies lockout at the configured limit.
    /// </summary>
    /// <param name="userId">The user whose failed attempt should be recorded.</param>
    /// <param name="maxFailedAttempts">The maximum attempts allowed before lockout.</param>
    /// <param name="now">The current UTC date and time.</param>
    /// <param name="lockedUntil">The UTC expiration for a newly applied lockout.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddFailedAttemptAsync(
        Guid userId,
        int maxFailedAttempts,
        DateTime now,
        DateTime lockedUntil,
        CancellationToken ct);

    /// <summary>
    /// Clears the persisted failed-attempt count and lockout.
    /// </summary>
    /// <param name="userId">The user whose attempt state should be reset.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ResetAttemptsAsync(Guid userId, CancellationToken ct);
}
