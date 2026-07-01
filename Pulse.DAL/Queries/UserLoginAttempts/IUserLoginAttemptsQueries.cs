using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Queries.UserLoginAttempts;

/// <summary>
/// Defines read operations for persisted user login attempt state.
/// </summary>
public interface IUserLoginAttemptsQueries : IQueries
{
    /// <summary>
    /// Retrieves the current login attempt state for a user.
    /// </summary>
    /// <param name="userId">The user whose login attempt state should be retrieved.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>
    /// The persisted attempt state, or <c>null</c> when the user has no attempt record.
    /// </returns>
    Task<UserLoginAttemptsRecord?> GetUserLoginAttemptsAsync(Guid userId, CancellationToken ct);
}
