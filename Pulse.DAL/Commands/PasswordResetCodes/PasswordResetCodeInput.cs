namespace Pulse.DAL.Commands.PasswordResetCodes;

/// <summary>
/// Input data required to create a new password reset code record.
/// </summary>
/// <param name="UserId">The ID of the user requesting a password reset.</param>
/// <param name="CodeHash">The hashed representation of the 6-digit OTP code.</param>
/// <param name="ExpiresAt">The UTC time at which the code expires.</param>
/// <param name="CreatedAt">The UTC time at which the code was created.</param>
public sealed record PasswordResetCodeInput(
    Guid UserId,
    string CodeHash,
    DateTimeOffset ExpiresAt,
    DateTimeOffset CreatedAt);
