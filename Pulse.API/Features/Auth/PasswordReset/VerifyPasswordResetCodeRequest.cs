namespace Pulse.API.Features.Auth.PasswordReset;

public sealed record VerifyPasswordResetCodeRequest(string Email, string Code);
