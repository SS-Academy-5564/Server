using Pulse.DAL.Common.Repository;
using Pulse.DAL.Queries.RefreshTokens;

namespace Pulse.DAL.Commands.RefreshTokens;

public interface IRefreshTokenCommands : ICommands
{
    /// <summary>
    /// Creates a new refresh token record in the database.
    /// </summary>
    /// <param name="record">The refresh token record to create.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task CreateAsync(RefreshTokenRecord record, CancellationToken ct);

    /// <summary>
    /// Updates an existing refresh token record in the database.
    /// </summary>
    /// <param name="record">The refresh token record to update.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task UpdateAsync(RefreshTokenRecord record, CancellationToken ct);
    /// <summary>
    /// Atomically rotates a refresh token by marking the old token as used and inserting a new token.
    /// </summary>
    /// <param name="oldRecord">The old refresh token record to mark as used.</param>
    /// <param name="newRecord">The new refresh token record to insert.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns><c>true</c> if the rotation was successful; otherwise, <c>false</c>.</returns>
    Task<bool> RotateAsync(RefreshTokenRecord oldRecord, RefreshTokenRecord newRecord, CancellationToken ct);
    /// <summary>
    /// Revokes all active refresh tokens associated with a specific family.
    /// </summary>
    /// <param name="familyId">The ID of the token family.</param>
    /// <param name="reason">The reason for revoking the tokens.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task RevokeFamilyAsync(Guid familyId, string reason, CancellationToken ct);

    /// <summary>
    /// Revokes all active refresh tokens associated with a specific user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="reason">The reason for revoking the tokens.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task RevokeAllForUserAsync(Guid userId, string reason, CancellationToken ct);
}
