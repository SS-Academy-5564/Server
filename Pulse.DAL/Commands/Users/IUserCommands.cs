using System.Data;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.Users;

public interface IUserCommands : ICommands
{
    // change to Task later when we will remove adding user to default organization
    Task<Guid> CreateUserAsync(CreateUserInput input, IDbTransaction transaction, CancellationToken ct);
}
