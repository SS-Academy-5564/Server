using FluentResults;
using Pulse.BL.Common.Handlers;

namespace Pulse.BL.Features.Auth.PasswordReset.RequestCode;

/// <summary>
/// Handles a password reset code request.
/// </summary>
public interface IRequestPasswordResetHandler : IAsyncHandler
{
    /// <summary>
    /// Processes a password reset request for the given email.
    /// Always returns success to prevent email enumeration.
    /// If the email exists, a 6-digit OTP code is generated and sent via email.
    /// </summary>
    /// <param name="command">The command containing the email address.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>Always a successful result.</returns>
    Task<Result> RequestAsync(RequestPasswordResetCommand command, CancellationToken ct);
}
