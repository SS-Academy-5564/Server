namespace Pulse.BL.Features.Auth.Refresh;

/// <summary>
/// Represents a command to refresh a user's authentication token.
/// </summary>
/// <param name="RefreshToken">The current refresh token to exchange.</param>
public sealed record RefreshCommand(string RefreshToken);
