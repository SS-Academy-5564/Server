using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.Users;

public interface IUserCommands : ICommands
{
    // change to Task later when we will remove adding user to default organization
    /// <summary>
    /// Inserts a new user record and returns the generated user ID.
    /// </summary>
    /// <param name="input">The data required to create the user.</param>
    /// <param name="uow">The unit of work providing the connection and transaction.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The <see cref="Guid"/> of the newly created user.</returns>
    /// <exception cref="Pulse.DAL.Exceptions.DuplicateKeyException">Thrown when a user with the same email already exists.</exception>
    Task<Guid> CreateUserAsync(CreateUserInput input, IUnitOfWork uow, CancellationToken ct);

    /// <summary>
    /// Updates the password hash for the specified user.
    /// </summary>
    /// <param name="userId">The ID of the user whose password should be updated.</param>
    /// <param name="passwordHash">The new hashed password.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task UpdatePasswordAsync(Guid userId, string passwordHash, CancellationToken ct);
}
