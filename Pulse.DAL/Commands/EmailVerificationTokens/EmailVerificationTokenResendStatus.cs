namespace Pulse.DAL.Commands.EmailVerificationTokens;

/// <summary>
/// Identifies the outcome of preparing a replacement email verification token.
/// </summary>
public enum EmailVerificationTokenResendStatus
{
    /// <summary>
    /// A replacement token was stored.
    /// </summary>
    Succeeded = 0,

    /// <summary>
    /// The presented token does not exist.
    /// </summary>
    Invalid = 1,

    /// <summary>
    /// The presented token is still valid and does not require replacement.
    /// </summary>
    NotExpired = 2,

    /// <summary>
    /// The presented token was consumed or its account is already verified.
    /// </summary>
    AlreadyUsed = 3,

    /// <summary>
    /// A replacement token was created too recently.
    /// </summary>
    Cooldown = 4
}
