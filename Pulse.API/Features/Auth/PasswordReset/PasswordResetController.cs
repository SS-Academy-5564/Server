using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Pulse.API.Attributes;
using Pulse.API.Constants;
using Pulse.API.Controllers;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.Auth.PasswordReset.RequestCode;
using Pulse.BL.Features.Auth.PasswordReset.ResetPassword;
using Pulse.BL.Features.Auth.PasswordReset.VerifyCode;

namespace Pulse.API.Features.Auth.PasswordReset;

[ApiController]
[Route("api/auth/password-reset")]
[AutoValidate]
public class PasswordResetController : PulseControllerBase
{
    private readonly IAsyncHandler<RequestPasswordResetCommand, Result> _requestHandler;
    private readonly IAsyncHandler<VerifyPasswordResetCodeCommand, Result<VerifyCodeResult>> _verifyHandler;
    private readonly IAsyncHandler<ResetPasswordCommand, Result> _resetHandler;

    public PasswordResetController(
        IAsyncHandler<RequestPasswordResetCommand, Result> requestHandler,
        IAsyncHandler<VerifyPasswordResetCodeCommand, Result<VerifyCodeResult>> verifyHandler,
        IAsyncHandler<ResetPasswordCommand, Result> resetHandler)
    {
        _requestHandler = requestHandler;
        _verifyHandler = verifyHandler;
        _resetHandler = resetHandler;
    }

    /// <summary>
    /// Requests a password reset code to be sent to the provided email.
    /// Always returns 200 OK to prevent email enumeration.
    /// </summary>
    /// <param name="request">The request containing the user's email address.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>200 OK regardless of whether the email exists.</returns>
    [HttpPost("request")]
    [EnableRateLimiting(RateLimitPolicies.PasswordReset)]
    public async Task<IActionResult> RequestCodeAsync(
        [Validate] RequestPasswordResetRequest request, CancellationToken ct)
    {
        RequestPasswordResetCommand command = new(request.Email);
        Result result = await _requestHandler.HandleAsync(command, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Verifies the 6-digit OTP code and returns a short-lived reset token on success.
    /// </summary>
    /// <param name="request">The request containing the user's email and the 6-digit code.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>200 OK with a reset token, or 400 Bad Request on invalid/expired code.</returns>
    [HttpPost("verify")]
    public async Task<IActionResult> VerifyCodeAsync(
        [Validate] VerifyPasswordResetCodeRequest request, CancellationToken ct)
    {
        VerifyPasswordResetCodeCommand command = new(request.Email, request.Code);
        Result<VerifyCodeResult> result = await _verifyHandler.HandleAsync(command, ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Changes the user's password using a valid reset token obtained from the verify step.
    /// </summary>
    /// <param name="request">The request containing the reset token and new password (confirmed).</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>200 OK on success, or 401 Unauthorized if the reset token is invalid or expired.</returns>
    [HttpPost("reset")]
    public async Task<IActionResult> ResetPasswordAsync(
        [Validate] ResetPasswordRequest request, CancellationToken ct)
    {
        ResetPasswordCommand command = new(request.ResetToken, request.NewPassword);
        Result result = await _resetHandler.HandleAsync(command, ct);
        return ToActionResult(result);
    }
}
