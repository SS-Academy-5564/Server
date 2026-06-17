namespace Pulse.BL.Features.Auth.Login;

public sealed class LoginResult
{
    public required string AccessToken { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
}
