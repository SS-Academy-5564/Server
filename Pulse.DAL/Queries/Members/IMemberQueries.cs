using Pulse.DAL.Common.Pagination;
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
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    Task<IReadOnlyList<UserOrganizationRecord>> GetOrganizationsByUserIdAsync(Guid userId, CancellationToken ct);

    /// <summary>
    /// Gets members for the specified organization.
    /// </summary>
    /// <param name="organizationId">The identifier of the organization.</param>
    /// <param name="pageNumber">The one-based page number.</param>
    /// <param name="pageSize">The maximum number of records to return.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The requested member records and total number of organization members.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    Task<PagedRecords<MemberRecord>> GetMembersByOrganizationIdAsync(
        Guid organizationId,
        int pageNumber,
        int pageSize,
        CancellationToken ct);
}
