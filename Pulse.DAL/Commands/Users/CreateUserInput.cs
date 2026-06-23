namespace Pulse.DAL.Commands.Users;

public record CreateUserInput(string Email, string FirstName, string LastName, string PasswordHash);
