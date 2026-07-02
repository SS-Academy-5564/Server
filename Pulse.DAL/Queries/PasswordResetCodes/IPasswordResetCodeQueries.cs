using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Queries.PasswordResetCodes;

/// <summary>
/// Defines read operations for password reset codes.
/// </summary>
public interface IPasswordResetCodeQueries : IQueries
{
    /// <summary>
    /// Retrieves the active (non-expired) password reset code for a given user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The active reset code record, or <c>null</c> if none exists.</returns>
    Task<PasswordResetCodeRecord?> GetActiveByUserIdAsync(Guid userId, CancellationToken ct);
}
