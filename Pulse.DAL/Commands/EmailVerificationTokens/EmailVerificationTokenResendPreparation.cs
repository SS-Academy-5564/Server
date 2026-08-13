namespace Pulse.DAL.Commands.EmailVerificationTokens;

/// <summary>
/// Describes whether an expired token was replaced and identifies its recipient.
/// </summary>
/// <param name="Status">The resend preparation outcome.</param>
/// <param name="Email">The recipient email when a replacement token was stored.</param>
public sealed record EmailVerificationTokenResendPreparation(
    EmailVerificationTokenResendStatus Status,
    string? Email);
