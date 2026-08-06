namespace Pulse.DAL.Commands.EmailVerificationTokens;

/// <summary>
/// Contains the persisted data for a newly issued email verification token.
/// </summary>
/// <param name="UserId">The user whose email address will be verified.</param>
/// <param name="TokenHash">The SHA-256 hash of the token sent to the user.</param>
/// <param name="ExpiresAt">The UTC time at which the token expires.</param>
/// <param name="CreatedAt">The UTC time at which the token was issued.</param>
public sealed record CreateEmailVerificationTokenInput(
    Guid UserId,
    string TokenHash,
    DateTimeOffset ExpiresAt,
    DateTimeOffset CreatedAt);
