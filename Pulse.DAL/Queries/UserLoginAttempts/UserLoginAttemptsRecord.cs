namespace Pulse.DAL.Queries.UserLoginAttempts;

/// <summary>
/// Represents the persisted login attempt state for a user.
/// </summary>
/// <param name="UserId">The user associated with the attempt state.</param>
/// <param name="AttemptCount">The number of currently reserved login attempts.</param>
/// <param name="LockedUntil">The UTC lockout expiration, or <c>null</c> when unlocked.</param>
public record UserLoginAttemptsRecord(Guid UserId, int AttemptCount, DateTime? LockedUntil);
