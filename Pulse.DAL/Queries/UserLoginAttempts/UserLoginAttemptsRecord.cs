namespace Pulse.DAL.Queries.UserLoginAttempts;

/// <summary>
/// Represents the persisted login attempt state for a user.
/// </summary>
/// <param name="UserId">The user associated with the attempt state.</param>
/// <param name="FailedAttempts">The number failed login attempts.</param>
/// <param name="LockedUntil">The UTC lockout expiration, or <c>null</c> when unlocked.</param>
public record UserLoginAttemptsRecord(Guid UserId, int FailedAttempts, DateTime? LockedUntil);
