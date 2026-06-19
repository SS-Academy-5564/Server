using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.Users;

public interface IUserCommands : ICommands
{
    // Returning the created identifier allows the caller to use the new user immediately.
    Task<Guid> CreateAsync(CreateUserInput input, CancellationToken ct);
}
