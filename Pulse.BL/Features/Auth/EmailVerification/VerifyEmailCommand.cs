namespace Pulse.BL.Features.Auth.EmailVerification;

/// <summary>
/// Represents a request to consume an email verification token.
/// </summary>
/// <param name="Token">The raw token received through the verification link.</param>
public sealed record VerifyEmailCommand(string Token);
