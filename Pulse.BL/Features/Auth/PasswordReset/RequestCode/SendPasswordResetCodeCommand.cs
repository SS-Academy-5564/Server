namespace Pulse.BL.Features.Auth.PasswordReset.RequestCode;

/// <summary>
/// Command for sending a password reset code to the given email.
/// </summary>
/// <param name="Email">The email address of the account to reset.</param>
public sealed record SendPasswordResetCodeCommand(string Email);
