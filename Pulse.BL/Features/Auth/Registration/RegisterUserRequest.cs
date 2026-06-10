
namespace Pulse.BL.Features.Auth.Registration;

public class RegisterUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
