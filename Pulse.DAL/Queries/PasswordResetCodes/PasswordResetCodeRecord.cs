namespace Pulse.DAL.Queries.PasswordResetCodes;

/// <summary>
/// Represents a password reset code record retrieved from the database.
/// </summary>
/// <param name="Id">The unique identifier of the reset code record.</param>
/// <param name="UserId">The ID of the user this code belongs to.</param>
/// <param name="CodeHash">The hashed 6-digit OTP code.</param>
/// <param name="ExpiresAt">The UTC time at which the code expires.</param>
/// <param name="FailedAttempts">The number of consecutive failed verification attempts.</param>
public sealed record PasswordResetCodeRecord(
    Guid Id,
    Guid UserId,
    string CodeHash,
    DateTimeOffset ExpiresAt,
    int FailedAttempts);
