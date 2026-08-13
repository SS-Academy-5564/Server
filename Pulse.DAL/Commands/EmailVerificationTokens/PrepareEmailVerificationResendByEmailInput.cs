namespace Pulse.DAL.Commands.EmailVerificationTokens;

/// <summary>
/// Contains the values required to replace verification tokens by email address.
/// </summary>
/// <param name="Email">The email address supplied by the requester.</param>
/// <param name="ReplacementTokenHash">The SHA-256 hash of the new token.</param>
/// <param name="RequestedAt">The UTC time at which the resend was requested.</param>
/// <param name="ReplacementExpiresAt">The UTC expiry time of the new token.</param>
/// <param name="ResendCooldownSeconds">The minimum interval between verification emails.</param>
public sealed record PrepareEmailVerificationResendByEmailInput(
    string Email,
    string ReplacementTokenHash,
    DateTimeOffset RequestedAt,
    DateTimeOffset ReplacementExpiresAt,
    int ResendCooldownSeconds);
