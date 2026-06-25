namespace Pulse.BL.Features.Auth.PasswordReset.RequestCode;

/// <summary>
/// Command for requesting a password reset code.
/// </summary>
/// <param name="Email">The email address of the account to reset.</param>
public sealed record RequestPasswordResetCommand(string Email);
