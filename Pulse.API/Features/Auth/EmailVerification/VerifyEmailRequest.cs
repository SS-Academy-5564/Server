namespace Pulse.API.Features.Auth.EmailVerification;

/// <summary>
/// Represents an email verification request.
/// </summary>
/// <param name="Token">The opaque token received through the verification link.</param>
public sealed record VerifyEmailRequest(string Token);
