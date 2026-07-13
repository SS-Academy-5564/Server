using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Queries.Members;

/// <summary>
/// Defines query operations for reading organization memberships.
/// </summary>
public interface IMemberQueries : IQueries
{
    /// <summary>
    /// Retrieves the organizations a user belongs to, together with the user's role in each.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>
    /// A list of the user's organization memberships ordered by <c>JoinedAt</c> ascending
    /// (oldest first), with a stable tiebreaker; an empty list when the user has none.
    /// </returns>
    Task<IReadOnlyList<UserOrganizationRecord>> GetOrganizationsByUserIdAsync(Guid userId, CancellationToken ct);
}
