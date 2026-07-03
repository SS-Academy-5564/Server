using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.Users;

public interface IUserCommands : ICommands
{
    // change to Task later when we will remove adding user to default organization
    /// <summary>
    /// Inserts a new user record and returns the generated user ID.
    /// </summary>
    /// <param name="input">The data required to create the user.</param>
    /// <param name="session">The session providing the connection and transaction.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The <see cref="Guid"/> of the newly created user.</returns>
    /// <exception cref="Pulse.DAL.Exceptions.DuplicateKeyException">Thrown when a user with the same email already exists.</exception>
    Task<Guid> CreateUserAsync(CreateUserInput input, IDbSession session, CancellationToken ct);

    /// <summary>
    /// Atomically consumes a one-time password reset token (JTI) and updates the password hash if valid.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="jti">The JWT ID of the reset token.</param>
    /// <param name="newPasswordHash">The new hashed password.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>True if the token was consumed and password updated, false otherwise.</returns>
    Task<bool> ConsumeResetTokenAndUpdatePasswordAsync(Guid userId, string jti, string newPasswordHash, CancellationToken ct);
}
