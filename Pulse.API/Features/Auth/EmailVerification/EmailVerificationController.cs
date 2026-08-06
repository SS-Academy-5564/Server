using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Attributes;
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
    private readonly IAsyncHandler<VerifyEmailCommand, Result> _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailVerificationController"/> class.
    /// </summary>
    /// <param name="handler">The handler that consumes verification tokens.</param>
    public EmailVerificationController(IAsyncHandler<VerifyEmailCommand, Result> handler)
    {
        _handler = handler;
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
        Result result = await _handler.HandleAsync(new VerifyEmailCommand(request.Token), ct);
        return ToActionResult(result);
    }
}
