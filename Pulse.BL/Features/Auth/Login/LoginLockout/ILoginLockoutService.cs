namespace Pulse.BL.Features.Auth.Login.LoginLockout;

/// <summary>
/// Defines account-level login lockout operations.
/// </summary>
public interface ILoginLockoutService
{

    /// <summary>
    /// Records a confirmed failed login attempt.
    /// </summary>
    /// <param name="userId">The user whose failed attempt should be recorded.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task AddFailedAttemptAsync(Guid userId, CancellationToken ct);

    /// <summary>
    /// Clears the attempt count and lockout after successful authentication.
    /// </summary>
    /// <param name="userId">The successfully authenticated user.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task ResetAttemptsAsync(Guid userId, CancellationToken ct);
}
