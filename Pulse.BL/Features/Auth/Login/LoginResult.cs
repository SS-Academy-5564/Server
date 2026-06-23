namespace Pulse.BL.Features.Auth.Login;

public sealed record LoginResult(string AccessToken, DateTimeOffset ExpiresAt);
