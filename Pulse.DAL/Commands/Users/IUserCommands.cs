namespace Pulse.DAL.Commands.Users;

public interface IUserCommands
{
    // change to Task later, so code will follow command/query segregation principles
    Task<Guid> CreateAsync(CreateUserInput input);
}
