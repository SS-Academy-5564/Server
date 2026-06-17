using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.Users;

public interface IUserCommands : ICommands
{
    // change to Task later, so code will follow command/query segregation principles
    Task<Guid> CreateAsync(CreateUserInput input, CancellationToken ct);
}
