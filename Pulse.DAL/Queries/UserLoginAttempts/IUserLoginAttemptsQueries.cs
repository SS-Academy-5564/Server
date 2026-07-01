using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Queries.UserLoginAttempts;

/// <summary>
/// Defines read operations for user login attempt state.
/// </summary>
public interface IUserLoginAttemptsQueries : IQueries
{
    /// <summary>
    /// Retrieves the current login attempt state for a user.
    /// </summary>
    /// <param name="userId">The user whose attempt state should be retrieved.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The attempt state, or <c>null</c> when no record exists.</returns>
    Task<UserLoginAttemptsRecord?> GetUserLoginAttemptsAsync(Guid userId, CancellationToken ct);
}
