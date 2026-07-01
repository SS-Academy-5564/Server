using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.LoginAttempts;

/// <summary>
/// Defines database commands for mutating user login attempt state.
/// </summary>
public interface IUserLoginAttemptsCommands : ICommands
{
    /// <summary>
    /// Atomically reserves an attempt and applies the account cooldown when the
    /// configured attempt limit is reached.
    /// </summary>
    /// <param name="userId">The user whose attempt is being reserved.</param>
    /// <param name="maxAttempts">The maximum allowed attempts before lockout.</param>
    /// <param name="now">The current UTC date and time.</param>
    /// <param name="lockedUntil">The UTC date and time at which a new lockout expires.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>
    /// <c>true</c> when the attempt was reserved; otherwise <c>false</c> when
    /// an existing lockout is still active.
    /// </returns>
    Task<bool> TryReserveLoginAttemptAsync(
        Guid userId,
        int maxAttempts,
        DateTime now,
        DateTime lockedUntil,
        CancellationToken ct);

    /// <summary>
    /// Resets the persisted login attempt state after successful authentication.
    /// </summary>
    /// <param name="userId">The user whose attempt state should be reset.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task ResetAttemptsAsync(Guid userId, CancellationToken ct);
}
