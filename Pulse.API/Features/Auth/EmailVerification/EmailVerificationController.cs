using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Attributes;
using Pulse.API.Common.Localization;
using Pulse.API.Controllers;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.Auth.EmailVerification;

namespace Pulse.API.Features.Auth.EmailVerification;

/// <summary>
/// Exposes email verification operations.
/// </summary>
[ApiController]
[Route("api/auth/email-verification")]
public sealed class EmailVerificationController : PulseControllerBase
{
    private readonly IAsyncHandler<VerifyEmailCommand, Result> _verifyHandler;
    private readonly IAsyncHandler<ResendEmailVerificationCommand, Result<ResendEmailVerificationResult>> _resendHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailVerificationController"/> class.
    /// </summary>
    /// <param name="verifyHandler">The handler that consumes verification tokens.</param>
    /// <param name="resendHandler">The handler that replaces expired verification tokens.</param>
    public EmailVerificationController(
        IAsyncHandler<VerifyEmailCommand, Result> verifyHandler,
        IAsyncHandler<ResendEmailVerificationCommand, Result<ResendEmailVerificationResult>> resendHandler)
    {
        _verifyHandler = verifyHandler;
        _resendHandler = resendHandler;
    }

    /// <summary>
    /// Verifies the email address associated with a one-time token.
    /// </summary>
    /// <param name="request">The request containing the token from the verification link.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>200 OK for a valid token, 400 for invalid or expired tokens, or 409 when already used.</returns>
    [HttpPost("verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> VerifyAsync([Validate] VerifyEmailRequest request, CancellationToken ct)
    {
        Result result = await _verifyHandler.HandleAsync(new VerifyEmailCommand(request.Token), ct);
        return ToActionResult(result);
    }

    /// <summary>
    /// Sends a replacement verification email for an expired one-time token.
    /// </summary>
    /// <param name="request">The request containing the expired token.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <param name="acceptLanguage">The preferred language supplied in the Accept-Language header.</param>
    /// <returns>200 OK with cooldown guidance, or a typed token-state or delivery failure.</returns>
    [HttpPost("resend")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> ResendAsync(
        [Validate] ResendEmailVerificationRequest request,
        CancellationToken ct,
        [FromHeader(Name = "Accept-Language")] string? acceptLanguage = null)
    {
        string language = EmailLanguageResolver.Resolve(acceptLanguage ?? Request.Headers.AcceptLanguage.ToString());
        Result<ResendEmailVerificationResult> result = await _resendHandler.HandleAsync(
            new ResendEmailVerificationCommand(request.Token, language),
            ct);

        return ToActionResult(result);
    }
}
