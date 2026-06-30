namespace Pulse.DAL.Queries.UserLoginAttempts;

public record UserLoginAttemptsRecord(Guid UserId, int FailedAttempts, DateTime? LockedUntil);
