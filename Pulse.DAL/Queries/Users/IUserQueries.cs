using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Queries.Users;

/// <summary>
/// Defines query operations for looking up user authentication data.
/// </summary>
public interface IUserQueries : IQueries
{
    /// <summary>
    /// Checks whether a user with the given email address already exists.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns><c>true</c> if a user with this email exists; otherwise <c>false</c>.</returns>
    Task<bool> EmailExistsAsync(string email, CancellationToken ct);

    /// <summary>
    /// Retrieves authentication information for a user by email address.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The authentication record for the user when found; otherwise <c>null</c>.</returns>
    Task<UserAuthRecord?> GetByEmailForAuthAsync(string email, CancellationToken ct);

    /// <summary>
    /// Retrieves the ID of a user by their email address.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The <see cref="Guid"/> of the user when found; otherwise <c>null</c>.</returns>
    Task<Guid?> GetIdByEmailAsync(string email, CancellationToken ct);
}
