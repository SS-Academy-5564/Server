namespace Pulse.API.Features.Auth.EmailVerification;

/// <summary>
/// Represents a request for a replacement email verification link.
/// </summary>
/// <param name="Token">The expired opaque token received through the verification link.</param>
public sealed record ResendEmailVerificationRequest(string Token);
