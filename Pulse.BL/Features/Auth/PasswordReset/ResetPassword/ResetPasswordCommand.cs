namespace Pulse.BL.Features.Auth.PasswordReset.ResetPassword;

/// <summary>
/// Command for changing a user's password after OTP verification.
/// </summary>
/// <param name="ResetToken">The short-lived signed reset token received from the verify step.</param>
/// <param name="NewPassword">The user's desired new password.</param>
public sealed record ResetPasswordCommand(string ResetToken, string NewPassword);
