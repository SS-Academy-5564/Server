using Pulse.DAL.Common.Repository;
using Pulse.DAL.Queries.RefreshTokens;

namespace Pulse.DAL.Commands.RefreshTokens;

public interface IRefreshTokenCommands : ICommands
{
    Task CreateAsync(RefreshTokenRecord record, CancellationToken ct);
    Task UpdateAsync(RefreshTokenRecord record, CancellationToken ct);
    /// <summary>
    /// Atomically rotates a refresh token by marking the old token as used and inserting a new token.
    /// </summary>
    /// <param name="oldRecord">The old refresh token record to mark as used.</param>
    /// <param name="newRecord">The new refresh token record to insert.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns><c>true</c> if the rotation was successful; otherwise, <c>false</c>.</returns>
    Task<bool> RotateAsync(RefreshTokenRecord oldRecord, RefreshTokenRecord newRecord, CancellationToken ct);
    Task RevokeFamilyAsync(Guid familyId, string reason, CancellationToken ct);
}
