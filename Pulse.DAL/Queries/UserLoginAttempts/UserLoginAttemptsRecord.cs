namespace Pulse.DAL.Queries.UserLoginAttempts;

public record UserLoginAttemptsRecord(Guid UserId, int AttemptCount, DateTime? LockedUntil);
