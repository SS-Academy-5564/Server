namespace Pulse.API.Features.Auth.PasswordReset;

public sealed record ResetPasswordRequest(
    string ResetToken,
    string NewPassword,
    string ConfirmPassword);
