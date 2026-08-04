using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.EmailVerificationTokens;

/// <summary>
/// Defines persistence operations for email verification tokens.
/// </summary>
public interface IEmailVerificationTokenCommands : ICommands
{
    /// <summary>
    /// Creates an email verification token in the active unit of work.
    /// </summary>
    /// <param name="input">The token data to persist.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task CreateAsync(CreateEmailVerificationTokenInput input, CancellationToken ct);

    /// <summary>
    /// Atomically consumes a token and marks the associated email address as verified.
    /// </summary>
    /// <param name="tokenHash">The SHA-256 hash of the presented token.</param>
    /// <param name="consumedAt">The UTC time at which consumption is attempted.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The outcome that identifies valid, invalid, expired, and already-used tokens.</returns>
    Task<EmailVerificationTokenConsumeResult> ConsumeAsync(
        string tokenHash,
        DateTimeOffset consumedAt,
        CancellationToken ct);
}
