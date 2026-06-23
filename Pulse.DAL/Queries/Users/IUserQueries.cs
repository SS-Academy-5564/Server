using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Queries.Users;

public interface IUserQueries : IQueries
{
    /// <summary>
    /// Checks whether a user with the given email address already exists.
    /// </summary>
    /// <param name="email">The email address to check.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns><c>true</c> if a user with this email exists; otherwise <c>false</c>.</returns>
    Task<bool> EmailExistsAsync(string email, CancellationToken ct);
    Task<UserAuthRecord?> GetByEmailForAuthAsync(string email, CancellationToken ct);
}
