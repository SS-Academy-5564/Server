namespace Pulse.DAL.Commands.EmailVerificationTokens;

/// <summary>
/// Describes the outcome of atomically consuming an email verification token.
/// </summary>
public enum EmailVerificationTokenConsumeResult
{
    /// <summary>
    /// The token was valid and the user's email address was marked as verified.
    /// </summary>
    Succeeded = 0,

    /// <summary>
    /// No token with the supplied hash exists.
    /// </summary>
    Invalid = 1,

    /// <summary>
    /// The token exists but its expiration time has passed.
    /// </summary>
    Expired = 2,

    /// <summary>
    /// The token was already consumed.
    /// </summary>
    AlreadyUsed = 3
}
