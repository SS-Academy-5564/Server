using System.Data;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.Users;

public interface IUserCommands : ICommands
{
    // change to Task later when we will remove adding user to default organization
    /// <summary>
    /// Inserts a new user record and returns the generated user ID.
    /// </summary>
    /// <param name="input">The data required to create the user.</param>
    /// <param name="transaction">The active transaction to execute the insert within.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The <see cref="Guid"/> of the newly created user.</returns>
    /// <exception cref="Pulse.DAL.Exceptions.DuplicateKeyException">Thrown when a user with the same email already exists.</exception>
    Task<Guid> CreateUserAsync(CreateUserInput input, IDbTransaction transaction, CancellationToken ct);
}
