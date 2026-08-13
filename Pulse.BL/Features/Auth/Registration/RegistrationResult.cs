namespace Pulse.BL.Features.Auth.Registration;

/// <summary>
/// Contains client guidance returned after registration succeeds.
/// </summary>
/// <param name="ResendCooldownSeconds">The configured delay before another verification email can be requested.</param>
public sealed record RegistrationResult(int ResendCooldownSeconds);
