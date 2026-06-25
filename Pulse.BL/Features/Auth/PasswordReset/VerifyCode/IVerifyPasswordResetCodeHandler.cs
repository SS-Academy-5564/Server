using FluentResults;
using Pulse.BL.Common.Handlers;

namespace Pulse.BL.Features.Auth.PasswordReset.VerifyCode;

/// <summary>
/// Handles OTP code verification for password reset.
/// </summary>
public interface IVerifyPasswordResetCodeHandler : IAsyncHandler
{
    /// <summary>
    /// Verifies the 6-digit OTP code for a password reset request.
    /// </summary>
    /// <param name="command">The command containing the email and code.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>
    /// A result containing a <see cref="VerifyCodeResult"/> with a short-lived reset token on success,
    /// or a failure result with an <c>InvalidCode</c> or <c>Timeout</c> error.
    /// </returns>
    Task<Result<VerifyCodeResult>> VerifyAsync(VerifyPasswordResetCodeCommand command, CancellationToken ct);
}
