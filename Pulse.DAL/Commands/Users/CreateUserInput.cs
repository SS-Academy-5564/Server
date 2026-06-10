
namespace Pulse.DAL.Commands.Users;

public class CreateUserInput
{
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
}
