namespace Pulse.BL.Features.Auth.EmailVerification;

/// <summary>
/// Requests a replacement email verification link for an expired token.
/// </summary>
/// <param name="Token">The expired token received through the verification link.</param>
/// <param name="Language">The preferred verification email language.</param>
public sealed record ResendEmailVerificationCommand(string Token, string Language);
