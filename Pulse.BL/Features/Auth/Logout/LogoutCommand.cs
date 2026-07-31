namespace Pulse.BL.Features.Auth.Logout;

/// <summary>
/// Represents a command to log out a user by revoking their refresh token.
/// </summary>
/// <param name="RefreshToken">The refresh token to revoke.</param>
public sealed record LogoutCommand(string? RefreshToken);
