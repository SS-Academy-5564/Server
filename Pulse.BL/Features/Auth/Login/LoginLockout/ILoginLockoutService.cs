namespace Pulse.BL.Features.Auth.Login.LoginLockout;

/// <summary>
/// Defines account-level login attempt reservation and reset operations.
/// </summary>
public interface ILoginLockoutService
{
    /// <summary>
    /// Atomically reserves one login attempt when the account is not locked.
    /// A successful login must reset the reserved attempt.
    /// </summary>
    /// <param name="userId">The user whose login attempt is being reserved.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>
    /// <c>true</c> when password verification may proceed; otherwise <c>false</c>
    /// while the account cooldown is active.
    /// </returns>
    Task<bool> TryReserveLoginAttemptAsync(Guid userId, CancellationToken ct);

    /// <summary>
    /// Clears the attempt count and account lock after a successful login.
    /// </summary>
    /// <param name="userId">The successfully authenticated user.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task ResetAttemptsAsync(Guid userId, CancellationToken ct);
}
