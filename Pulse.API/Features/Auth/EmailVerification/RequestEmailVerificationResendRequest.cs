namespace Pulse.API.Features.Auth.EmailVerification;

/// <summary>
/// Represents a request for another account verification email.
/// </summary>
/// <param name="Email">The email address supplied by the requester.</param>
public sealed record RequestEmailVerificationResendRequest(string Email);
