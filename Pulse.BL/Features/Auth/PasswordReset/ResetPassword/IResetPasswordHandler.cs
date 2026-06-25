using FluentResults;
using Pulse.BL.Common.Handlers;

namespace Pulse.BL.Features.Auth.PasswordReset.ResetPassword;

/// <summary>
/// Handles the final password reset step — changing the user's password.
/// </summary>
public interface IResetPasswordHandler : IAsyncHandler
{
    /// <summary>
    /// Validates the reset token and updates the user's password.
    /// </summary>
    /// <param name="command">The command containing the reset token and new password.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A successful result, or a failure if the token is invalid or expired.</returns>
    Task<Result> ResetAsync(ResetPasswordCommand command, CancellationToken ct);
}
