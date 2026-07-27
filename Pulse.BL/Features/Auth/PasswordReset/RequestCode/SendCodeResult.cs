namespace Pulse.BL.Features.Auth.PasswordReset.RequestCode;

/// <summary>
/// Represents the result of a successful password reset code request.
/// </summary>
/// <param name="ResendCooldownSeconds">The number of seconds the client must wait before requesting a new code.</param>
public sealed record SendCodeResult(int ResendCooldownSeconds);
