namespace Pulse.DAL.Commands.EmailVerificationTokens;

/// <summary>
/// Contains the values required to replace an expired email verification token.
/// </summary>
/// <param name="PresentedTokenHash">The SHA-256 hash of the expired token presented by the client.</param>
/// <param name="ReplacementTokenHash">The SHA-256 hash of the new token.</param>
/// <param name="RequestedAt">The UTC time at which the resend was requested.</param>
/// <param name="ReplacementExpiresAt">The UTC expiry time of the new token.</param>
/// <param name="ResendCooldownSeconds">The minimum interval between replacement tokens.</param>
public sealed record PrepareEmailVerificationTokenResendInput(
    string PresentedTokenHash,
    string ReplacementTokenHash,
    DateTimeOffset RequestedAt,
    DateTimeOffset ReplacementExpiresAt,
    int ResendCooldownSeconds);
