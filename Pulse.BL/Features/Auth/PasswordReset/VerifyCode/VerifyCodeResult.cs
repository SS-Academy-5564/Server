namespace Pulse.BL.Features.Auth.PasswordReset.VerifyCode;

/// <summary>
/// Represents the result of a successful OTP verification.
/// </summary>
/// <param name="ResetToken">A short-lived signed token authorizing the password reset step.</param>
public sealed record VerifyCodeResult(string ResetToken);
