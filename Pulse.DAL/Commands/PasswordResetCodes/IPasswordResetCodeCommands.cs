using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.PasswordResetCodes;

/// <summary>
/// Defines write operations for password reset codes.
/// </summary>
public interface IPasswordResetCodeCommands : ICommands
{
    /// <summary>
    /// Inserts a new password reset code record and returns the generated ID.
    /// </summary>
    /// <param name="input">The data required to create the reset code.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The <see cref="Guid"/> of the newly created record.</returns>
    Task<Guid> ReplaceAsync(PasswordResetCodeInput input, CancellationToken ct);

    /// <summary>
    /// Marks a password reset code as verified by replacing its hash with a JWT ID (JTI).
    /// This prevents the 6-digit code from being reused, while preserving the row for the final atomic reset.
    /// </summary>
    /// <param name="id">The ID of the reset code.</param>
    /// <param name="jti">The unique identifier of the JWT.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>True if exactly one row was updated.</returns>
    Task<bool> MarkAsVerifiedAsync(Guid id, string jti, CancellationToken ct);

    /// <summary>
    /// Deletes all password reset codes for the specified user.
    /// </summary>
    /// <param name="userId">The user whose codes should be deleted.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    Task DeleteByUserIdAsync(Guid userId, CancellationToken ct);

    /// <summary>
    /// Increments the failed attempt counter for a specific reset code and returns the new count.
    /// </summary>
    /// <param name="id">The ID of the reset code record.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The updated number of failed attempts.</returns>
    Task<int> IncrementFailedAttemptsAsync(Guid id, CancellationToken ct);
}
