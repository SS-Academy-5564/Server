namespace Pulse.BL.Features.Auth.PasswordReset.VerifyCode;

/// <summary>
/// Command for verifying an OTP password reset code.
/// </summary>
/// <param name="Email">The email address of the user.</param>
/// <param name="Code">The 6-digit code entered by the user.</param>
public sealed record VerifyPasswordResetCodeCommand(string Email, string Code);
