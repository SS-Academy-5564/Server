namespace Pulse.BL.Features.Auth.EmailVerification;

/// <summary>
/// Requests a new email verification link without disclosing account state.
/// </summary>
/// <param name="Email">The email address supplied by the requester.</param>
/// <param name="Language">The preferred verification email language.</param>
public sealed record RequestEmailVerificationResendCommand(string Email, string Language);
