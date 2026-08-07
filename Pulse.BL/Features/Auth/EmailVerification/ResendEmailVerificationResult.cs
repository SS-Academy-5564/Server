namespace Pulse.BL.Features.Auth.EmailVerification;

/// <summary>
/// Contains client guidance returned after a replacement verification email is sent.
/// </summary>
/// <param name="ResendCooldownSeconds">The number of seconds before another resend may be requested.</param>
public sealed record ResendEmailVerificationResult(int ResendCooldownSeconds);
