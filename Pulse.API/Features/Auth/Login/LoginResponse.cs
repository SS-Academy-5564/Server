namespace Pulse.API.Features.Auth.Login;

public sealed record LoginResponse(string AccessToken, DateTimeOffset ExpiresAt);
